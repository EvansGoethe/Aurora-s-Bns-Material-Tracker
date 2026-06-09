using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
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
        public string               RawOcrText     { get; set; } = "";      // for debug display
        public List<string>         DetectedTokens { get; set; } = new();   // item-color word tokens
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

            // ── Pass 1: Full-image OCR ──────────────────────────────────────
            // Extracts dungeon name, party size, mode, section headers with
            // their approximate Y-positions in the original image.
            var ocrResult = await engine.RecognizeAsync(soft);
            var result    = Parse(ocrResult.Text);
            result.RawOcrText = ocrResult.Text;

            var allWords = ocrResult.Lines.SelectMany(l => l.Words).ToList();

            // Dungeon name: topmost-leftmost short pure-CJK word
            var titleWord = allWords
                .Where(w => Regex.IsMatch(w.Text.Trim(), @"[一-鿿㐀-䶿]"))
                .OrderBy(w => w.BoundingRect.Top)
                .ThenBy(w => w.BoundingRect.Left)
                .FirstOrDefault();
            if (titleWord != null)
            {
                string wt = titleWord.Text.Trim();
                if (Regex.IsMatch(wt, @"^[一-鿿㐀-䶿]{2,6}$"))
                    result.DungeonName = wt;
            }

            // Locate each section header's Y-coordinate in the image
            var headerAnchors = FindHeaderAnchors(allWords, result.Sections);

            // ── Pass 2: Color-filtered OCR ─────────────────────────────────
            // BnS item names are rendered in non-white colored text (orange, gold,
            // blue, etc.), while section headers and description text are white.
            // Isolate only the colored pixels → black-on-white for OCR, so OCR
            // reads item names without interference from lore/description text.
            await EnrichSectionsFromColor(engine, screenshot, result, headerAnchors);

            return result;
        }

        // ── Section header Y-anchor detection ──────────────────────────────
        // For each parsed section, find the OCR word whose text best matches the
        // section's header label, and record its Y coordinate as an anchor.

        private static Dictionary<string, double> FindHeaderAnchors(
            IReadOnlyList<OcrWord> words, List<DungeonSection> sections)
        {
            var anchors = new Dictionary<string, double>();

            foreach (var sec in sections)
            {
                string label = sec.Label; // e.g. "共通獎勵"
                OcrWord? best    = null;
                int      bestLen = 0;

                foreach (var w in words)
                {
                    string wt = w.Text.Replace(" ", "");
                    // Slide a window over wt looking for the longest substring
                    // that is also a substring of the known label.
                    for (int len = Math.Min(wt.Length, label.Length); len >= 2; len--)
                    {
                        for (int start = 0; start <= wt.Length - len; start++)
                        {
                            if (label.Contains(wt.Substring(start, len), StringComparison.Ordinal)
                                && len > bestLen)
                            {
                                bestLen = len;
                                best    = w;
                            }
                        }
                    }
                }

                if (best != null && bestLen >= 2)
                    anchors[sec.Difficulty] = best.BoundingRect.Top;
            }

            return anchors;
        }

        // ── Color-filtered OCR pass ─────────────────────────────────────────

        private static async Task EnrichSectionsFromColor(
            OcrEngine engine,
            BitmapSource screenshot,
            DungeonScanResult result,
            Dictionary<string, double> headerAnchors)
        {
            // Ordered (anchorY, difficulty) list for section assignment
            var sortedAnchors = headerAnchors
                .Select(kv => (Y: kv.Value, Diff: kv.Key))
                .OrderBy(a => a.Y)
                .ToList();

            // Filter image → keep only colored (item-name) pixels, rest → white
            var filteredBmp  = IsolateItemText(screenshot);
            var filteredSoft = await ToBitmapAsync(filteredBmp);
            if (filteredSoft == null) return;

            var filteredOcr = await engine.RecognizeAsync(filteredSoft);

            var sectionItemsMap = result.Sections.ToDictionary(s => s.Difficulty, _ => new List<string>());
            var allColorTokens  = new List<string>();

            foreach (var word in filteredOcr.Lines.SelectMany(l => l.Words))
            {
                // Normalise: strip spaces OCR sometimes inserts between chars
                string t = Regex.Replace(word.Text.Trim(), @"\s+", "");
                if (t.Length < 2 || t.Length > 14) continue;
                if (!Regex.IsMatch(t, @"[一-鿿㐀-䶿]{2,}")) continue;
                if (KnownHeaderWords.Contains(t)) continue;
                if (IsNoiseItem(t)) continue;

                allColorTokens.Add(t);

                if (sortedAnchors.Count > 0)
                {
                    // Assign word to the section whose header is just above it
                    double wordY         = word.BoundingRect.Top + word.BoundingRect.Height / 2.0;
                    string assignedDiff  = sortedAnchors[0].Diff;
                    foreach (var (anchorY, diff) in sortedAnchors)
                    {
                        if (anchorY <= wordY) assignedDiff = diff;
                        else break;
                    }
                    if (sectionItemsMap.ContainsKey(assignedDiff))
                        sectionItemsMap[assignedDiff].Add(t);
                }
            }

            // Push colour-detected items into each section
            foreach (var sec in result.Sections)
            {
                if (sectionItemsMap.TryGetValue(sec.Difficulty, out var items) && items.Count > 0)
                {
                    var distinct = items.Distinct().ToList();
                    sec.ItemNames = distinct;
                    // Prepend to RawText so DataEditorView's substring matching works
                    sec.RawText = string.Join("", distinct) + sec.RawText;
                }
            }

            result.DetectedTokens = allColorTokens.Distinct().ToList();
        }

        // ── Colour isolation ────────────────────────────────────────────────
        // BnS reward-item names use non-white colored text (orange/gold/blue, etc.).
        // Description text and section headers are near-white.
        // Algorithm: for each pixel convert to HSV; if saturation > threshold and
        // brightness in mid range → colored text → output black; else → output white.
        // Result: black item-name glyphs on white background, ideal for OCR.

        private static BitmapSource IsolateItemText(BitmapSource src)
        {
            var fmt = src.Format == PixelFormats.Bgra32
                ? src
                : new FormatConvertedBitmap(src, PixelFormats.Bgra32, null, 0);

            int w = fmt.PixelWidth, h = fmt.PixelHeight;
            var pixels = new byte[w * h * 4];
            fmt.CopyPixels(pixels, w * 4, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte bv = pixels[i], gv = pixels[i + 1], rv = pixels[i + 2];

                float maxC = Math.Max(rv, Math.Max(gv, bv)) / 255f;
                float minC = Math.Min(rv, Math.Min(gv, bv)) / 255f;
                float sat  = maxC > 0.001f ? (maxC - minC) / maxC : 0f;
                float bri  = maxC;

                // Colored text:  non-trivial saturation, not too dark, not pure white
                // White text:    sat ≈ 0, bri ≈ 1 → excluded
                // Dark bg:       bri < 0.28 → excluded
                bool isColored = sat > 0.22f && bri > 0.28f && bri < 0.97f;

                byte val = isColored ? (byte)0 : (byte)255;  // black text / white bg
                pixels[i]     = val;
                pixels[i + 1] = val;
                pixels[i + 2] = val;
                pixels[i + 3] = 255;
            }

            double dpiX = src.DpiX > 0 ? src.DpiX : 96.0;
            double dpiY = src.DpiY > 0 ? src.DpiY : 96.0;
            return BitmapSource.Create(w, h, dpiX, dpiY, PixelFormats.Bgra32, null, pixels, w * 4);
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
