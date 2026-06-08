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
    public class ScanResult
    {
        public string MaterialId { get; set; } = "";
        public string MatName    { get; set; } = "";
        public string MatIcon    { get; set; } = "📦";
        public int    Quantity   { get; set; } = -1;
        public bool   Found      { get; set; } = false;
        public double MatchScore { get; set; } = 0;
    }

    public static class BagScanService
    {
        // Templates are stored as TemplateSize×TemplateSize Bgr32 images.
        public const int TemplateSize = 40;

        // MSE per channel below this = match.
        // Identical images → 0; same icon, slight JPEG noise → ~100-800;
        // different icons → typically > 3000.
        public const double MseThreshold = 1800.0;

        // ── Template creation ──────────────────────────────────────────────

        /// <summary>
        /// Crop a TemplateSize×TemplateSize region centered at (cx, cy) from the screenshot.
        /// Shifts 4px upward to avoid the quantity number at the very bottom-left of the cell.
        /// Returns raw Bgr32 bytes (TemplateSize * TemplateSize * 4).
        /// </summary>
        public static byte[] CreateTemplate(BitmapSource screenshot, int cx, int cy)
        {
            int x0 = Math.Max(0, cx - TemplateSize / 2);
            int y0 = Math.Max(0, cy - TemplateSize / 2 - 4);   // shift up 4px
            int w  = Math.Min(TemplateSize, screenshot.PixelWidth  - x0);
            int h  = Math.Min(TemplateSize, screenshot.PixelHeight - y0);

            if (w <= 0 || h <= 0) return Array.Empty<byte>();

            BitmapSource region = new CroppedBitmap(screenshot, new Int32Rect(x0, y0, w, h));

            if (w != TemplateSize || h != TemplateSize)
                region = new TransformedBitmap(region,
                    new ScaleTransform((double)TemplateSize / w, (double)TemplateSize / h));

            var fc = new FormatConvertedBitmap(region, PixelFormats.Bgr32, null, 0);
            var pixels = new byte[TemplateSize * TemplateSize * 4];
            fc.CopyPixels(pixels, TemplateSize * 4, 0);
            return pixels;
        }

        // ── Pixel extraction ───────────────────────────────────────────────

        public static (byte[] pixels, int stride) GetPixels(BitmapSource src)
        {
            var fc = new FormatConvertedBitmap(src, PixelFormats.Bgr32, null, 0);
            int stride = fc.PixelWidth * 4;
            var px = new byte[stride * fc.PixelHeight];
            fc.CopyPixels(px, stride, 0);
            return (px, stride);
        }

        // ── Template matching (MSE) ────────────────────────────────────────

        /// <summary>
        /// Compute mean squared error per channel between the stored 40×40 template
        /// and the 40×40 pixel region at (sx, sy) in the screenshot.
        /// This is a direct 1-to-1 pixel comparison — no scaling.
        /// </summary>
        private static double RegionMse(
            byte[] ssPixels, int ssStride, int ssW, int ssH,
            byte[] tmplPixels, int sx, int sy)
        {
            if (sx < 0 || sy < 0 || sx + TemplateSize > ssW || sy + TemplateSize > ssH)
                return double.MaxValue;

            double sum = 0;
            for (int ty = 0; ty < TemplateSize; ty++)
            {
                int si_row = (sy + ty) * ssStride + sx * 4;
                int ti_row = ty * TemplateSize * 4;
                for (int tx = 0; tx < TemplateSize; tx++)
                {
                    int si = si_row + tx * 4;
                    int ti = ti_row + tx * 4;
                    // B, G, R  (skip alpha at [+3])
                    double db = ssPixels[si]     - tmplPixels[ti];
                    double dg = ssPixels[si + 1] - tmplPixels[ti + 1];
                    double dr = ssPixels[si + 2] - tmplPixels[ti + 2];
                    sum += db * db + dg * dg + dr * dr;
                }
            }
            // Divide by number of values (TemplateSize*TemplateSize*3 channels)
            return sum / (TemplateSize * TemplateSize * 3.0);
        }

        private struct SearchHit { public int X, Y; public double Mse; }

        /// <summary>
        /// Search for the template in the screenshot.
        /// Returns the top-left (x,y) of the best-matching 40×40 region and its MSE.
        /// </summary>
        private static SearchHit FindBestMatch(
            byte[] ssPixels, int ssStride, int ssW, int ssH,
            byte[] tmplPixels,
            int cx, int cy,   // original click center from registration
            bool fullScan)
        {
            // The template was captured starting at (cx - TSize/2, cy - TSize/2 - 4)
            int expectedX = cx - TemplateSize / 2;
            int expectedY = cy - TemplateSize / 2 - 4;

            int coarseStep = fullScan ? 5 : 2;
            int x0, x1, y0, y1;

            if (fullScan)
            {
                x0 = 0;                        x1 = ssW - TemplateSize;
                y0 = 0;                        y1 = ssH - TemplateSize;
            }
            else
            {
                int r = 150;   // ±150px — handles moderate window drift
                x0 = Math.Max(0,                   expectedX - r);
                x1 = Math.Min(ssW - TemplateSize,  expectedX + r);
                y0 = Math.Max(0,                   expectedY - r);
                y1 = Math.Min(ssH - TemplateSize,  expectedY + r);
            }

            var best = new SearchHit { X = expectedX, Y = expectedY, Mse = double.MaxValue };

            for (int y = y0; y <= y1; y += coarseStep)
                for (int x = x0; x <= x1; x += coarseStep)
                {
                    double mse = RegionMse(ssPixels, ssStride, ssW, ssH, tmplPixels, x, y);
                    if (mse < best.Mse) { best.Mse = mse; best.X = x; best.Y = y; }
                }

            // Fine pass at step 1 around the best coarse hit
            int fx0 = Math.Max(0,                  best.X - coarseStep - 1);
            int fx1 = Math.Min(ssW - TemplateSize,  best.X + coarseStep + 1);
            int fy0 = Math.Max(0,                  best.Y - coarseStep - 1);
            int fy1 = Math.Min(ssH - TemplateSize,  best.Y + coarseStep + 1);

            for (int y = fy0; y <= fy1; y++)
                for (int x = fx0; x <= fx1; x++)
                {
                    double mse = RegionMse(ssPixels, ssStride, ssW, ssH, tmplPixels, x, y);
                    if (mse < best.Mse) { best.Mse = mse; best.X = x; best.Y = y; }
                }

            return best;
        }

        // ── OCR ───────────────────────────────────────────────────────────

        /// <summary>
        /// Read the quantity number from the bottom-left of the matched region.
        /// matchX, matchY = top-left of the 40×40 matched region in the screenshot.
        /// The number sits in the lower ~40% of the cell, left ~60%.
        /// We crop that area, upscale ×5, threshold white pixels, then OCR.
        /// </summary>
        public static async Task<int> ReadQuantityAsync(
            BitmapSource screenshot, int matchX, int matchY, int cellSize)
        {
            // Number region relative to the matched top-left:
            //   - vertically: bottom 42% of cell → but template top is ~4px above cell center
            //   - use empirical offsets tuned for BnS bag at typical resolutions
            int nx = Math.Max(0, matchX);
            int ny = Math.Max(0, matchY + TemplateSize / 2);   // lower half of template
            int nw = Math.Min(TemplateSize * 3 / 4, screenshot.PixelWidth  - nx);
            int nh = Math.Min(TemplateSize / 2,     screenshot.PixelHeight - ny);

            if (nw < 4 || nh < 4) return -1;

            var cropped = new CroppedBitmap(screenshot, new Int32Rect(nx, ny, nw, nh));

            // Scale up ×5 so digits are large enough for OCR (min ~40px tall recommended)
            var upscaled = new TransformedBitmap(cropped, new ScaleTransform(5, 5));

            // Threshold: keep bright pixels (the white digit outlines)
            var thresholded = ThresholdWhite(upscaled, brightnessMin: 145);

            var softBmp = await ToBitmapAsync(thresholded);
            if (softBmp == null) return -1;

            var engine = OcrEngine.TryCreateFromUserProfileLanguages()
                      ?? TryAnyEngine();
            if (engine == null) return -1;

            var result = await engine.RecognizeAsync(softBmp);
            string text = result.Text.Trim();

            // Extract contiguous digits (handles commas, spaces, misread chars)
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
                var enc = new PngBitmapEncoder();
                enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(src));
                var ms = new MemoryStream();
                enc.Save(ms);
                ms.Position = 0;
                var ras = ms.AsRandomAccessStream();
                var dec = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(ras);
                return await dec.GetSoftwareBitmapAsync(
                    BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }
            catch { return null; }
        }

        // ── Full scan ──────────────────────────────────────────────────────

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
                                        tmpl.CenterX, tmpl.CenterY, fullScan);

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
