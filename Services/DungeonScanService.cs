using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

namespace BnsMaterialTracker.Services
{
    public class DungeonSection
    {
        public string       Difficulty { get; set; } = "";  // "common" | "easy"/"normal"/"skilled" | "1"-"7"
        public string       Label      { get; set; } = "";  // original Chinese header text
        public string       RawText    { get; set; } = "";  // compact (whitespace-free) section body for substring matching
        public List<string> ItemNames  { get; set; } = new();
    }

    public class DungeonScanResult
    {
        public string               DungeonName    { get; set; } = "";
        public int                  PartySize      { get; set; } = 0;
        public string               Mode           { get; set; } = "hero";  // "hero" | "demon"
        public List<DungeonSection> Sections       { get; set; } = new();
        public string               RawOcrText     { get; set; } = "";      // for debugging
        public List<string>         DetectedTokens { get; set; } = new();   // word-level OCR tokens (material candidates)
    }

    public static class DungeonScanService
    {
        // ── Public API ─────────────────────────────────────────────────────

        public static async Task<DungeonScanResult?> ScanAsync(BitmapSource screenshot)
        {
            var engine = TryChineseEngine()
                      ?? OcrEngine.TryCreateFromUserProfileLanguages()
                      ?? TryAnyEngine();
            if (engine == null) return null;

            var soft = await ToBitmapAsync(screenshot);
            if (soft == null) return null;

            var ocrResult = await engine.RecognizeAsync(soft);
            var result    = Parse(ocrResult.Text);
            result.RawOcrText = ocrResult.Text;

            // ── Word-level post-processing ──────────────────────────────────
            var allWords = ocrResult.Lines.SelectMany(l => l.Words).ToList();

            // Dungeon name: use the topmost-leftmost CJK word (title text in BnS UI
            // is always at the top-left in a larger font, so it's a separate OcrWord).
            var titleWord = allWords
                .Where(w => Regex.IsMatch(w.Text.Trim(), @"[一-鿿㐀-䶿]"))
                .OrderBy(w => w.BoundingRect.Top)
                .ThenBy(w => w.BoundingRect.Left)
                .FirstOrDefault();

            if (titleWord != null)
            {
                string wt = titleWord.Text.Trim();
                // Accept as name only if it's a short pure-CJK token
                if (Regex.IsMatch(wt, @"^[一-鿿㐀-䶿]{2,6}$"))
                    result.DungeonName = wt;
                // else fall back to the Parse-based extraction already done
            }

            // Material candidates: word-level tokens that look like item names
            result.DetectedTokens = allWords
                .Select(w => w.Text.Trim())
                .Where(t => Regex.IsMatch(t, @"^[一-鿿㐀-䶿][一-鿿㐀-䶿\d]*[一-鿿㐀-䶿]$"))
                .Where(t => t.Length >= 2 && t.Length <= 14)
                .Where(t => !KnownHeaderWords.Contains(t) && !IsNoiseItem(t))
                .Distinct()
                .ToList();

            return result;
        }

        private static readonly HashSet<string> KnownHeaderWords = new()
        {
            "共通獎勵","入門獎勵","一般獎勵","熟練獎勵","熱練獎勵",
            "共通類","入門類","一般類","熟練類","熱練類",
            "其他","服裝","其他服裝",
        };

        // ── Parser ─────────────────────────────────────────────────────────
        //
        // Windows OCR does not guarantee one section-header per line: the newline
        // boundary may fall inside "入門\n獎勵", and text at the same visual Y-level
        // gets merged into one OCR line.
        //
        // Strategy: strip ALL whitespace from the OCR output to form a compact
        // string, then find known section-header substrings by index position.
        // The text between two adjacent headers is stored verbatim as RawText;
        // the view does material matching by searching known material names as
        // substrings within RawText — no splitting required.

        internal static DungeonScanResult Parse(string rawText)
        {
            var result = new DungeonScanResult();

            // Collapse all whitespace so split headers like "入門\n獎勵" reunite
            string compact = Regex.Replace(rawText, @"\s", "");

            // ── Locate section headers ──────────────────────────────────────
            // Include 熱練 (OCR: 熟→熱) and X類 (OCR: 獎勵→類) variants
            var headerDefs = new[]
            {
                ("共通獎勵", "common"),   ("共通類", "common"),
                ("入門獎勵", "easy"),     ("入門類", "easy"),
                ("一般獎勵", "normal"),   ("一般類", "normal"),
                ("熟練獎勵", "skilled"),  ("熟練類", "skilled"),
                ("熱練獎勵", "skilled"),  ("熱練類", "skilled"),
            };

            var found     = new List<(int idx, int len, string diff, string label)>();
            var usedDiffs = new HashSet<string>();

            foreach (var (hdr, diff) in headerDefs)
            {
                if (usedDiffs.Contains(diff)) continue;
                int idx = compact.IndexOf(hdr, StringComparison.Ordinal);
                if (idx >= 0) { found.Add((idx, hdr.Length, diff, hdr)); usedDiffs.Add(diff); }
            }

            // Demon mode: "封魔X段獎勵"
            foreach (Match dm in Regex.Matches(compact, @"封魔(\d)段獎勵"))
            {
                string d = dm.Groups[1].Value;
                if (!usedDiffs.Contains(d))
                {
                    found.Add((dm.Index, dm.Length, d, dm.Value));
                    usedDiffs.Add(d);
                }
            }

            found.Sort((a, b) => a.idx.CompareTo(b.idx));

            // ── Dungeon name ────────────────────────────────────────────────
            // The dungeon name is the first short CJK sequence, but OCR merges it
            // with the description that follows (e.g. "火田民村由於雷雲聚集之…").
            // We detect where the description begins using common openers and cut there.
            int firstHdrPos = found.Count > 0 ? found[0].idx : compact.Length;
            string preamble = compact[..Math.Min(firstHdrPos, compact.Length)];
            var cjkM = Regex.Match(preamble, @"[一-鿿㐀-䶿]{2,}");
            result.DungeonName = cjkM.Success ? TrimAtDescStart(cjkM.Value) : "";

            // ── Party size ──────────────────────────────────────────────────
            var psm = Regex.Match(rawText, @"(\d+)人");
            if (psm.Success) result.PartySize = int.Parse(psm.Groups[1].Value);

            // ── Mode ────────────────────────────────────────────────────────
            if (found.Any(f => f.diff is "easy" or "normal" or "skilled"))
                result.Mode = "hero";
            else if (found.Any(f => Regex.IsMatch(f.diff, @"^\d$")))
                result.Mode = "demon";

            // ── Sections ────────────────────────────────────────────────────
            for (int i = 0; i < found.Count; i++)
            {
                var (idx, len, diff, label) = found[i];
                int start = idx + len;
                int end   = i + 1 < found.Count ? found[i + 1].idx : compact.Length;

                string secRaw = compact[start..end];

                var sec = new DungeonSection { Difficulty = diff, Label = label, RawText = secRaw };

                // Best-effort token list for the preview count display
                string noised = secRaw.Replace("其他", "").Replace("服裝", "");
                foreach (Match m in Regex.Matches(noised, @"[一-鿿㐀-䶿]{2,}"))
                {
                    string t = m.Value;
                    if (!IsNoiseItem(t)) sec.ItemNames.Add(t);
                }

                result.Sections.Add(sec);
            }

            return result;
        }

        // Common phrase openers that start the dungeon description (not part of the name)
        private static readonly string[] DescStarters =
            { "由於", "由于", "自從", "自从", "以上", "入場", "為了", "可以統", "也可以" };

        private static string TrimAtDescStart(string cjk)
        {
            foreach (var ph in DescStarters)
            {
                int i = cjk.IndexOf(ph, StringComparison.Ordinal);
                if (i >= 2 && i <= 8) return cjk[..i];   // name is 2-8 chars max
            }
            return cjk.Length <= 6 ? cjk : cjk[..5];
        }

        private static bool IsNoiseItem(string t)
        {
            string[] noise = { "以上", "等級", "統合配對", "可以統合", "入場條件", "為了", "而建", "所到之處", "引發", "焦土化", "寄生" };
            foreach (var n in noise) if (t.Contains(n)) return true;
            return false;
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private static OcrEngine? TryChineseEngine()
        {
            foreach (var tag in new[] { "zh-TW", "zh-Hant", "zh-Hans", "zh-CN", "zh" })
            {
                try
                {
                    var lang = new Windows.Globalization.Language(tag);
                    if (OcrEngine.IsLanguageSupported(lang))
                    {
                        var e = OcrEngine.TryCreateFromLanguage(lang);
                        if (e != null) return e;
                    }
                }
                catch { }
            }
            return null;
        }

        private static OcrEngine? TryAnyEngine()
        {
            foreach (var lang in OcrEngine.AvailableRecognizerLanguages)
            {
                var e = OcrEngine.TryCreateFromLanguage(lang);
                if (e != null) return e;
            }
            return null;
        }

        private static async Task<SoftwareBitmap?> ToBitmapAsync(BitmapSource src)
        {
            try
            {
                var enc = new System.Windows.Media.Imaging.PngBitmapEncoder();
                enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(src));
                using var ms = new MemoryStream();
                enc.Save(ms);
                ms.Position = 0;
                var ras = ms.AsRandomAccessStream();
                var dec = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(ras);
                return await dec.GetSoftwareBitmapAsync(
                    BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }
            catch { return null; }
        }
    }
}
