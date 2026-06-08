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
        public string MaterialId  { get; set; } = "";
        public string MatName     { get; set; } = "";
        public string MatIcon     { get; set; } = "📦";
        public int    Quantity    { get; set; } = -1;
        public bool   Found       { get; set; } = false;
        public double MatchScore  { get; set; } = 0;   // NCC 0-1
        public int    FoundX      { get; set; } = 0;   // match top-left in screenshot
        public int    FoundY      { get; set; } = 0;
    }

    public static class BagScanService
    {
        /// <summary>Template size in pixels (both width and height).</summary>
        public const int TemplateSize = 40;

        /// <summary>
        /// NCC threshold: 0 = random, 1 = identical.
        /// 0.70 catches same icon under different screenshots/compression.
        /// Raise if false-positives occur; lower if misses occur.
        /// </summary>
        public const double NccThreshold = 0.70;

        // ── Template creation ──────────────────────────────────────────────

        /// <summary>
        /// Crop a TemplateSize×TemplateSize region centered at (cx, cy).
        /// Returns raw Bgr32 bytes (TemplateSize * TemplateSize * 4).
        /// </summary>
        public static byte[] CreateTemplate(BitmapSource screenshot, int cx, int cy)
        {
            int x0 = Math.Max(0, cx - TemplateSize / 2);
            int y0 = Math.Max(0, cy - TemplateSize / 2);
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

        // ── NCC matching ───────────────────────────────────────────────────

        /// <summary>
        /// Normalized Cross-Correlation between the 40×40 template and the
        /// 40×40 region at (sx, sy) in the screenshot.
        /// Returns a value in [-1, 1].  +1 = identical (ignoring brightness offset).
        /// Returns -1.0 if out of bounds.
        /// </summary>
        private static double RegionNcc(
            byte[] ssPixels, int ssStride, int ssW, int ssH,
            byte[] tmplPixels,
            int sx, int sy)
        {
            if (sx < 0 || sy < 0 ||
                sx + TemplateSize > ssW || sy + TemplateSize > ssH)
                return -1.0;

            const int N = TemplateSize * TemplateSize * 3; // total channel values

            // ── Pass 1: compute means ──────────────────────────────────────
            double sumA = 0, sumB = 0;
            for (int ty = 0; ty < TemplateSize; ty++)
            {
                int siRow = (sy + ty) * ssStride + sx * 4;
                int tiRow = ty * TemplateSize * 4;
                for (int tx = 0; tx < TemplateSize; tx++)
                {
                    int si = siRow + tx * 4;
                    int ti = tiRow + tx * 4;
                    sumA += ssPixels[si]     + ssPixels[si + 1] + ssPixels[si + 2];
                    sumB += tmplPixels[ti]   + tmplPixels[ti + 1] + tmplPixels[ti + 2];
                }
            }
            double meanA = sumA / N;
            double meanB = sumB / N;

            // ── Pass 2: NCC numerator and denominators ─────────────────────
            double num = 0, denA = 0, denB = 0;
            for (int ty = 0; ty < TemplateSize; ty++)
            {
                int siRow = (sy + ty) * ssStride + sx * 4;
                int tiRow = ty * TemplateSize * 4;
                for (int tx = 0; tx < TemplateSize; tx++)
                {
                    int si = siRow + tx * 4;
                    int ti = tiRow + tx * 4;
                    for (int c = 0; c < 3; c++)
                    {
                        double a = ssPixels[si + c]   - meanA;
                        double b = tmplPixels[ti + c] - meanB;
                        num  += a * b;
                        denA += a * a;
                        denB += b * b;
                    }
                }
            }

            double den = Math.Sqrt(denA * denB);
            return den < 1e-10 ? 0.0 : num / den;
        }

        // ── Search ────────────────────────────────────────────────────────

        private struct SearchHit { public int X, Y; public double Ncc; }

        private static SearchHit FindBestMatch(
            byte[] ssPixels, int ssStride, int ssW, int ssH,
            byte[] tmplPixels,
            int cx, int cy,
            bool fullScan)
        {
            // Expected position: template top-left
            int expectedX = cx - TemplateSize / 2;
            int expectedY = cy - TemplateSize / 2;

            int coarseStep = fullScan ? 4 : 2;

            int x0, x1, y0, y1;
            if (fullScan)
            {
                x0 = 0;  x1 = ssW - TemplateSize;
                y0 = 0;  y1 = ssH - TemplateSize;
            }
            else
            {
                int r = 160;
                x0 = Math.Max(0,                  expectedX - r);
                x1 = Math.Min(ssW - TemplateSize, expectedX + r);
                y0 = Math.Max(0,                  expectedY - r);
                y1 = Math.Min(ssH - TemplateSize, expectedY + r);
            }

            var best = new SearchHit { X = expectedX, Y = expectedY, Ncc = -1.0 };

            // Coarse pass
            for (int y = y0; y <= y1; y += coarseStep)
                for (int x = x0; x <= x1; x += coarseStep)
                {
                    double ncc = RegionNcc(ssPixels, ssStride, ssW, ssH, tmplPixels, x, y);
                    if (ncc > best.Ncc) { best.Ncc = ncc; best.X = x; best.Y = y; }
                }

            // Fine pass ±coarseStep around best coarse hit
            int fx0 = Math.Max(0,                  best.X - coarseStep);
            int fx1 = Math.Min(ssW - TemplateSize,  best.X + coarseStep);
            int fy0 = Math.Max(0,                  best.Y - coarseStep);
            int fy1 = Math.Min(ssH - TemplateSize,  best.Y + coarseStep);
            for (int y = fy0; y <= fy1; y++)
                for (int x = fx0; x <= fx1; x++)
                {
                    double ncc = RegionNcc(ssPixels, ssStride, ssW, ssH, tmplPixels, x, y);
                    if (ncc > best.Ncc) { best.Ncc = ncc; best.X = x; best.Y = y; }
                }

            return best;
        }

        // ── OCR ───────────────────────────────────────────────────────────

        /// <summary>
        /// Try to read the quantity number from the screenshot at the matched cell.
        /// matchX, matchY = top-left of the 40×40 matched region.
        /// </summary>
        public static async Task<int> ReadQuantityAsync(
            BitmapSource screenshot, int matchX, int matchY, int cellSize)
        {
            // The number is at the bottom-left of the CELL.
            // Cell centre = (matchX + TemplateSize/2, matchY + TemplateSize/2).
            // Cell extends cellSize/2 below the centre, but the template only goes
            // TemplateSize/2 below — so there are (cellSize-TemplateSize)/2 pixels
            // that lie BELOW the template bottom edge.
            int extra    = Math.Max(0, (cellSize - TemplateSize) / 2);  // e.g. 8 for cell=56
            int cellHalf = cellSize / 2;                                  // 28

            // Cell centre is at (matchX + TemplateSize/2, matchY + TemplateSize/2).
            // Cell bottom  = cell-centre-Y + cellSize/2
            //              = matchY + TemplateSize/2 + cellSize/2
            // For cell=56 : matchY + 20 + 28 = matchY + 48
            // The quantity number sits in the bottom ~20% of the cell.
            int cellCenterY = matchY + TemplateSize / 2;          // matchY+20
            int cellBottomY = cellCenterY + cellSize / 2;         // matchY+48
            int cellLeftX   = matchX + TemplateSize / 2 - cellSize / 2; // matchX-8
            int numH        = Math.Max(14, cellSize * 22 / 100);  // ~12 px for cell=56

            // Three crop attempts: all anchored at the actual cell bottom
            var candidates = new[]
            {
                // (absX, absY, w, h) — all absolute image coordinates
                (cellLeftX, cellBottomY - numH,     cellSize * 3 / 5, numH + 2),
                (cellLeftX, cellBottomY - numH - 4, cellSize * 2 / 3, numH + 6),
                (cellLeftX, cellCenterY + 4,        cellSize * 2 / 3, cellBottomY - cellCenterY - 2),
            };

            foreach (var (ax, ay, aw, ah) in candidates)
            {
                int nx = Math.Max(0, ax);
                int ny = Math.Max(0, ay);
                int nw = Math.Min(aw, screenshot.PixelWidth  - nx);
                int nh = Math.Min(ah, screenshot.PixelHeight - ny);
                if (nw < 4 || nh < 4) continue;

                int qty = await TryOcrCrop(screenshot, nx, ny, nw, nh);
                if (qty >= 0) return qty;
            }
            return -1;
        }

        private static int _debugCropIndex = 0;

        private static async Task<int> TryOcrCrop(
            BitmapSource src, int nx, int ny, int nw, int nh)
        {
            var cropped  = new CroppedBitmap(src, new Int32Rect(nx, ny, nw, nh));
            // ×8 upscale — bigger = easier for OCR to read small digits
            var upscaled = new TransformedBitmap(cropped, new ScaleTransform(8, 8));

            // Save debug crops to Desktop so we can inspect what OCR sees
            try
            {
                string path = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"bns_ocr_debug_{++_debugCropIndex:D2}.png");
                var enc = new PngBitmapEncoder();
                enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(upscaled));
                using var fs = System.IO.File.Create(path);
                enc.Save(fs);
            }
            catch { /* best-effort */ }

            var engine = OcrEngine.TryCreateFromUserProfileLanguages()
                      ?? TryAnyEngine();
            if (engine == null) return -1;

            // Attempt 1: raw color image (no thresholding) — WinRT OCR handles colors fine
            {
                var soft = await ToBitmapAsync(upscaled);
                if (soft != null)
                {
                    var result = await engine.RecognizeAsync(soft);
                    string digits = Regex.Replace(result.Text, @"[^\d]", "");
                    if (int.TryParse(digits, out int qty) && qty >= 0) return qty;
                }
            }

            // Attempts 2-5: "true white" threshold (min of all 3 channels >= t)
            // BnS numbers are white (R≥t & G≥t & B≥t); avoids orange/yellow backgrounds
            int[] trueWhiteThresholds = { 200, 180, 160, 140 };
            foreach (int t in trueWhiteThresholds)
            {
                var thresh = ThresholdTrueWhite(upscaled, t);
                var soft   = await ToBitmapAsync(thresh);
                if (soft == null) continue;
                var result = await engine.RecognizeAsync(soft);
                string digits = Regex.Replace(result.Text, @"[^\d]", "");
                if (int.TryParse(digits, out int qty) && qty >= 0) return qty;
            }

            // Attempt 6: luminance fallback (for edge cases)
            foreach (int t in new[] { 170, 140 })
            {
                var thresh = ThresholdLuminance(upscaled, t);
                var soft   = await ToBitmapAsync(thresh);
                if (soft == null) continue;
                var result = await engine.RecognizeAsync(soft);
                string digits = Regex.Replace(result.Text, @"[^\d]", "");
                if (int.TryParse(digits, out int qty) && qty >= 0) return qty;
            }

            return -1;
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
        /// "True white" threshold: pixel is white only when ALL channels ≥ t.
        /// BnS numbers are pure white; orange/yellow icons have low B channel → excluded.
        /// </summary>
        private static BitmapSource ThresholdTrueWhite(BitmapSource src, int t)
        {
            var fc = new FormatConvertedBitmap(src, PixelFormats.Bgr32, null, 0);
            int w = fc.PixelWidth, h = fc.PixelHeight, stride = w * 4;
            var px = new byte[stride * h];
            fc.CopyPixels(px, stride, 0);
            for (int i = 0; i < px.Length; i += 4)
            {
                // B=px[i], G=px[i+1], R=px[i+2]
                byte v = (px[i] >= t && px[i+1] >= t && px[i+2] >= t) ? (byte)255 : (byte)0;
                px[i] = px[i+1] = px[i+2] = v;
                px[i+3] = 255;
            }
            var wb = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgr32, null);
            wb.WritePixels(new Int32Rect(0, 0, w, h), px, stride, 0);
            return wb;
        }

        private static BitmapSource ThresholdLuminance(BitmapSource src, int minLum)
        {
            var fc = new FormatConvertedBitmap(src, PixelFormats.Bgr32, null, 0);
            int w = fc.PixelWidth, h = fc.PixelHeight, stride = w * 4;
            var px = new byte[stride * h];
            fc.CopyPixels(px, stride, 0);
            for (int i = 0; i < px.Length; i += 4)
            {
                int lum = (px[i] + px[i+1] + px[i+2]) / 3;
                byte v = lum >= minLum ? (byte)255 : (byte)0;
                px[i] = px[i+1] = px[i+2] = v;
                px[i+3] = 255;
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
                    FoundX     = hit.X,
                    FoundY     = hit.Y,
                };

                if (hit.Ncc >= NccThreshold)
                {
                    r.Found      = true;
                    r.MatchScore = hit.Ncc;
                    r.Quantity   = await ReadQuantityAsync(screenshot, hit.X, hit.Y, cellSize);
                }

                results.Add(r);
            }
            return results;
        }
    }
}
