using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BnsMaterialTracker.Models;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

namespace BnsMaterialTracker.Services
{
    /// <summary>Result of scanning one registered template in a screenshot.</summary>
    public class ScanResult
    {
        public string MaterialId { get; set; } = "";
        public string MatName    { get; set; } = "";
        public string MatIcon    { get; set; } = "📦";
        public int    Quantity   { get; set; } = -1;  // -1 = not found / OCR failed
        public bool   Found      { get; set; } = false;
        public double MatchScore { get; set; } = 0;   // 0-1, higher = more confident
    }

    public static class BagScanService
    {
        // ── Constants ─────────────────────────────────────────────────

        /// <summary>Templates are stored as 40×40 Bgr32 images.</summary>
        public const int TemplateSize = 40;

        /// <summary>Match below this MSE (per channel) is considered a hit.</summary>
        public const double MseThreshold = 1500.0;

        // ── Template creation ─────────────────────────────────────────

        /// <summary>
        /// Crop a 40×40 region centered at (cx, cy), slightly above center
        /// to avoid the quantity number in the bottom-left of the cell.
        /// Returns raw Bgr32 bytes (TemplateSize * TemplateSize * 4).
        /// </summary>
        public static byte[] CreateTemplate(BitmapSource screenshot, int cx, int cy)
        {
            // Shift up slightly so number (bottom-left) is not included
            int y = cy - 4;
            int x0 = Math.Max(0, cx - TemplateSize / 2);
            int y0 = Math.Max(0, y  - TemplateSize / 2);
            int w  = Math.Min(TemplateSize, screenshot.PixelWidth  - x0);
            int h  = Math.Min(TemplateSize, screenshot.PixelHeight - y0);

            if (w <= 0 || h <= 0) return Array.Empty<byte>();

            BitmapSource region = new CroppedBitmap(screenshot, new Int32Rect(x0, y0, w, h));

            // Resize to exactly TemplateSize × TemplateSize if needed
            if (w != TemplateSize || h != TemplateSize)
                region = new TransformedBitmap(region,
                    new ScaleTransform((double)TemplateSize / w, (double)TemplateSize / h));

            var fc = new FormatConvertedBitmap(region, PixelFormats.Bgr32, null, 0);
            var pixels = new byte[TemplateSize * TemplateSize * 4];
            fc.CopyPixels(pixels, TemplateSize * 4, 0);
            return pixels;
        }

        // ── Pixel extraction ──────────────────────────────────────────

        /// <summary>Returns (Bgr32 byte array, stride) for the full screenshot.</summary>
        public static (byte[] pixels, int stride) GetPixels(BitmapSource src)
        {
            var fc = new FormatConvertedBitmap(src, PixelFormats.Bgr32, null, 0);
            int stride = fc.PixelWidth * 4;
            var px = new byte[stride * fc.PixelHeight];
            fc.CopyPixels(px, stride, 0);
            return (px, stride);
        }

        // ── Template matching ─────────────────────────────────────────

        /// <summary>
        /// Compute mean squared error (per channel) between a TemplateSize×TemplateSize
        /// region in the screenshot (top-left at sx,sy) and the stored template pixels.
        /// Lower = more similar. Returns double.MaxValue if out of bounds.
        /// </summary>
        private static double RegionMse(
            byte[] ssPixels, int ssStride, int ssW, int ssH,
            byte[] tmplPixels,
            int sx, int sy, int cellSize)
        {
            double sum   = 0;
            int    count = 0;
            double scale = (double)cellSize / TemplateSize;  // pixels per template pixel

            for (int ty = 0; ty < TemplateSize; ty++)
            {
                int py = sy + (int)(ty * scale);
                if (py < 0 || py >= ssH) return double.MaxValue;

                for (int tx = 0; tx < TemplateSize; tx++)
                {
                    int px = sx + (int)(tx * scale);
                    if (px < 0 || px >= ssW) return double.MaxValue;

                    int ti = (ty * TemplateSize + tx) * 4;
                    int si = py * ssStride + px * 4;

                    // B, G, R channels
                    for (int c = 0; c < 3; c++)
                    {
                        double d = tmplPixels[ti + c] - ssPixels[si + c];
                        sum += d * d;
                    }
                    count += 3;
                }
            }
            return count > 0 ? sum / count : double.MaxValue;
        }

        private struct SearchHit { public int X, Y; public double Mse; }

        /// <summary>
        /// Search a region of the screenshot for the best match to the template.
        /// searchRadius = 0 means full screenshot; otherwise ±searchRadius around (cx,cy).
        /// Returns top-left pixel of best match and its MSE.
        /// </summary>
        private static SearchHit FindBestMatch(
            byte[] ssPixels, int ssStride, int ssW, int ssH,
            byte[] tmplPixels,
            int cx, int cy, int cellSize,
            bool fullScan)
        {
            int step = fullScan ? 8 : 4;

            int x0, x1, y0, y1;
            if (fullScan)
            {
                x0 = 0;  x1 = ssW - cellSize;
                y0 = 0;  y1 = ssH - cellSize;
            }
            else
            {
                int r = 100;
                x0 = Math.Max(0, cx - cellSize / 2 - r);
                x1 = Math.Min(ssW - cellSize, cx - cellSize / 2 + r);
                y0 = Math.Max(0, cy - cellSize / 2 - r);
                y1 = Math.Min(ssH - cellSize, cy - cellSize / 2 + r);
            }

            var best = new SearchHit
            {
                X = cx - cellSize / 2, Y = cy - cellSize / 2,
                Mse = double.MaxValue
            };

            for (int y = y0; y <= y1; y += step)
                for (int x = x0; x <= x1; x += step)
                {
                    double mse = RegionMse(ssPixels, ssStride, ssW, ssH, tmplPixels, x, y, cellSize);
                    if (mse < best.Mse) { best.Mse = mse; best.X = x; best.Y = y; }
                }

            // Refinement pass at step 1 around the best coarse position
            int rx0 = Math.Max(0, best.X - step * 2);
            int rx1 = Math.Min(ssW - cellSize, best.X + step * 2);
            int ry0 = Math.Max(0, best.Y - step * 2);
            int ry1 = Math.Min(ssH - cellSize, best.Y + step * 2);

            for (int y = ry0; y <= ry1; y++)
                for (int x = rx0; x <= rx1; x++)
                {
                    double mse = RegionMse(ssPixels, ssStride, ssW, ssH, tmplPixels, x, y, cellSize);
                    if (mse < best.Mse) { best.Mse = mse; best.X = x; best.Y = y; }
                }

            return best;
        }

        // ── OCR ───────────────────────────────────────────────────────

        /// <summary>
        /// Reads the quantity number from the bottom-left corner of an item cell.
        /// cellX, cellY = top-left of the cell in the screenshot.
        /// Returns -1 if OCR fails or number cannot be parsed.
        /// </summary>
        public static async Task<int> ReadQuantityAsync(
            BitmapSource screenshot, int cellX, int cellY, int cellSize)
        {
            // Number sits in approximately the bottom 32%, left 58% of cell
            int nx = Math.Max(0, cellX);
            int ny = Math.Max(0, cellY + (int)(cellSize * 0.66));
            int nw = Math.Min((int)(cellSize * 0.60), screenshot.PixelWidth  - nx);
            int nh = Math.Min((int)(cellSize * 0.34), screenshot.PixelHeight - ny);

            if (nw < 4 || nh < 4) return -1;

            var cropped = new CroppedBitmap(screenshot, new Int32Rect(nx, ny, nw, nh));

            // Scale up ×4 so OCR can read small digits reliably
            var upscaled = new TransformedBitmap(cropped, new ScaleTransform(4, 4));

            // Threshold: keep bright (white) pixels, zero out everything else
            var thresholded = ThresholdWhite(upscaled, brightnessMin: 150);

            var softBmp = await ToBitmapAsync(thresholded);
            if (softBmp == null) return -1;

            // Try installed languages; digits read fine with any language
            var engine = OcrEngine.TryCreateFromUserProfileLanguages()
                      ?? TryAnyEngine();
            if (engine == null) return -1;

            var result = await engine.RecognizeAsync(softBmp);
            string text = result.Text.Trim();

            // Strip everything except digits (handles commas, spaces, misread chars)
            string digits = Regex.Replace(text, @"[^\d]", "");
            return int.TryParse(digits, out int qty) ? qty : -1;
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

        /// <summary>
        /// Threshold: pixels with luminance > brightnessMin become white, rest black.
        /// Helps OCR read white text on complex icon backgrounds.
        /// </summary>
        private static BitmapSource ThresholdWhite(BitmapSource src, int brightnessMin)
        {
            var fc = new FormatConvertedBitmap(src, PixelFormats.Bgr32, null, 0);
            int w = fc.PixelWidth, h = fc.PixelHeight;
            int stride = w * 4;
            var px = new byte[stride * h];
            fc.CopyPixels(px, stride, 0);

            for (int i = 0; i < px.Length; i += 4)
            {
                int lum = (px[i] + px[i + 1] + px[i + 2]) / 3;
                byte v = lum >= brightnessMin ? (byte)255 : (byte)0;
                px[i] = px[i + 1] = px[i + 2] = v;
                px[i + 3] = 255;
            }

            var wb = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgr32, null);
            wb.WritePixels(new Int32Rect(0, 0, w, h), px, stride, 0);
            return wb;
        }

        private static async Task<SoftwareBitmap?> ToBitmapAsync(BitmapSource src)
        {
            try
            {
                // Encode to PNG in memory using WPF encoder
                var enc = new PngBitmapEncoder();
                enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(src));
                var ms = new MemoryStream();
                enc.Save(ms);
                ms.Position = 0;

                // Decode using WinRT BitmapDecoder
                var ras = ms.AsRandomAccessStream();
                var dec = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(ras);
                return await dec.GetSoftwareBitmapAsync(
                    BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }
            catch { return null; }
        }

        // ── Full scan orchestration ───────────────────────────────────

        /// <summary>
        /// Scan the screenshot against all registered templates.
        /// Returns one ScanResult per template.
        /// </summary>
        public static async Task<List<ScanResult>> ScanAsync(
            BitmapSource screenshot,
            IEnumerable<BagTemplate> templates,
            int cellSize,
            bool fullScan,
            Func<string, string> getName,
            Func<string, string> getIcon)
        {
            var results = new List<ScanResult>();
            var (px, stride) = GetPixels(screenshot);
            int w = screenshot.PixelWidth, h = screenshot.PixelHeight;

            foreach (var tmpl in templates)
            {
                var tpx = tmpl.PixelData;
                if (tpx.Length == 0) continue;

                var hit = FindBestMatch(px, stride, w, h, tpx,
                                        tmpl.CenterX, tmpl.CenterY, cellSize, fullScan);

                var r = new ScanResult
                {
                    MaterialId = tmpl.MaterialId,
                    MatName    = getName(tmpl.MaterialId),
                    MatIcon    = getIcon(tmpl.MaterialId),
                };

                if (hit.Mse <= MseThreshold)
                {
                    r.Found      = true;
                    r.MatchScore = Math.Max(0, 1.0 - hit.Mse / MseThreshold);
                    r.Quantity   = await ReadQuantityAsync(screenshot, hit.X, hit.Y, cellSize);
                }

                results.Add(r);
            }
            return results;
        }
    }
}
