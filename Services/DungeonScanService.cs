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
        public List<string> ItemNames  { get; set; } = new();
    }

    public class DungeonScanResult
    {
        public string               DungeonName { get; set; } = "";
        public int                  PartySize   { get; set; } = 0;
        public string               Mode        { get; set; } = "hero";  // "hero" | "demon"
        public List<DungeonSection> Sections    { get; set; } = new();
    }

    public static class DungeonScanService
    {
        private static readonly Dictionary<string, string> HeroSectionMap = new()
        {
            ["共通獎勵"] = "common",
            ["入門獎勵"] = "easy",
            ["一般獎勵"] = "normal",
            ["熟練獎勵"] = "skilled",
        };

        private static readonly HashSet<string> SkipTokens = new()
        {
            "其他", "服裝", "其他服裝",
        };

        private static readonly string[] NoiseKeywords =
        {
            "以上", "等級", "章：", "統合配對", "可以統合", "入場條件",
            "段：", "搭配武器", "為了", "而建", "所到之處", "引發",
        };

        // ── Public API ─────────────────────────────────────────────────────

        public static async Task<DungeonScanResult?> ScanAsync(BitmapSource screenshot)
        {
            var engine = OcrEngine.TryCreateFromUserProfileLanguages()
                      ?? TryAnyEngine();
            if (engine == null) return null;

            var soft = await ToBitmapAsync(screenshot);
            if (soft == null) return null;

            var ocrResult = await engine.RecognizeAsync(soft);
            return Parse(ocrResult.Text);
        }

        // ── Parser ─────────────────────────────────────────────────────────

        internal static DungeonScanResult Parse(string rawText)
        {
            var lines = rawText
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToList();

            var result = new DungeonScanResult();

            // ── Dungeon name: first short, non-noise line before any reward header ──
            foreach (var line in lines)
            {
                if (line.Contains("獎勵")) break;
                if (line.Length < 2 || line.Length > 20) continue;
                if (line == "副本" || line == "可以統合配對") continue;
                if (IsNoiseLine(line)) continue;
                result.DungeonName = line;
                break;
            }

            // ── Party size ──────────────────────────────────────────────────
            foreach (var line in lines)
            {
                var m = Regex.Match(line, @"(\d+)人");
                if (m.Success) { result.PartySize = int.Parse(m.Groups[1].Value); break; }
            }

            // ── Mode + reward sections ──────────────────────────────────────
            DungeonSection? current = null;

            foreach (var line in lines)
            {
                // Hero section header
                if (HeroSectionMap.TryGetValue(line, out string? diff))
                {
                    if (diff != "common") result.Mode = "hero";
                    current = new DungeonSection { Difficulty = diff, Label = line };
                    result.Sections.Add(current);
                    continue;
                }

                // Demon section header: "封魔X段獎勵"
                var demonM = Regex.Match(line, @"封魔(\d)段獎勵");
                if (demonM.Success)
                {
                    result.Mode = "demon";
                    current = new DungeonSection
                    {
                        Difficulty = demonM.Groups[1].Value,
                        Label = line,
                    };
                    result.Sections.Add(current);
                    continue;
                }

                if (current == null || IsNoiseLine(line)) continue;

                // Split line into tokens — items are space-separated in OCR output
                foreach (var token in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    string t = token.Trim();
                    if (t.Length >= 2 && !SkipTokens.Contains(t) && !IsNoiseLine(t))
                        current.ItemNames.Add(t);
                }
            }

            return result;
        }

        private static bool IsNoiseLine(string line)
        {
            if (line.Length <= 1) return true;
            foreach (var kw in NoiseKeywords)
                if (line.Contains(kw)) return true;
            if (Regex.IsMatch(line, @"^[\d\s。，、.]+$")) return true;
            return false;
        }

        // ── Helpers ────────────────────────────────────────────────────────

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
