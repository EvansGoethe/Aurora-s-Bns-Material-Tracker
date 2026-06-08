using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BnsMaterialTracker.Models;
using BnsMaterialTracker.Services;
using BnsMaterialTracker.ViewModels;
using Microsoft.Win32;

namespace BnsMaterialTracker.Views
{
    public partial class BagScanView : UserControl
    {
        // ── State ──────────────────────────────────────────────────────
        private AppViewModel  _vm          = null!;
        private BitmapSource? _screenshot;   // currently loaded screenshot (full res)
        private int           _pendingX;     // image coords of last click, waiting for material pick
        private int           _pendingY;
        private bool          _picking;      // picker overlay visible?
        private int           _cellSize     = 64;
        private List<ScanResult> _lastResults = new();

        // ── Helper display models ──────────────────────────────────────

        private class TemplateRow
        {
            public string MaterialId  { get; set; } = "";
            public string MatDisplay  { get; set; } = "";  // "📦 玄晶"
            public BitmapSource? Preview { get; set; }
        }

        private class ResultRow
        {
            public string MatIcon       { get; set; } = "📦";
            public string MatName       { get; set; } = "";
            public int    Quantity      { get; set; } = -1;
            public bool   Found         { get; set; } = false;
            public double MatchScore    { get; set; } = 0;

            public string QuantityDisplay =>
                Found && Quantity >= 0  ? Quantity.ToString("N0") :
                Found                   ? "?" :
                                          L10n.T("bagscan.notFound");

            public Brush QuantityColor =>
                Found && Quantity >= 0 ? Brushes.LimeGreen :
                Found                  ? Brushes.Orange :
                                         Brushes.Gray;

            /// <summary>Shows the NCC match score, e.g. "98%".</summary>
            public string ScoreDisplay  =>
                Found ? $"({MatchScore:P0})" : "";
            public Brush ScoreColor => MatchScore >= 0.90 ? Brushes.LimeGreen
                                     : MatchScore >= 0.75 ? Brushes.Yellow
                                     : Brushes.Orange;
        }

        // ── Init ───────────────────────────────────────────────────────

        public BagScanView() => InitializeComponent();

        public void Refresh()
        {
            if (DataContext is not AppViewModel vm) return;
            _vm = vm;
            ApplyL10n();
            // Restore saved cell size
            _cellSize = vm.Settings.BagCellSize > 0 ? vm.Settings.BagCellSize : 64;
            TxtCellSize.Text = _cellSize.ToString();
            RebuildTemplateList();
            UpdateScanButton();
        }

        private void ApplyL10n()
        {
            TxtTitle.Text         = L10n.T("bagscan.title");
            BtnImport.Content     = "📂 " + L10n.T("bagscan.import");
            TxtClickHint.Text     = L10n.T("bagscan.clickHint");
            TxtCellSizeLabel.Text = L10n.T("bagscan.cellSize");
            TxtTemplatesLabel.Text= L10n.T("bagscan.templates");
            TxtNoTemplates.Text   = L10n.T("bagscan.noTemplates");
            ChkFullScan.Content   = L10n.T("bagscan.fullScan");
            BtnScan.Content       = "🔍 " + L10n.T("bagscan.scan");
            BtnApply.Content      = "✅ " + L10n.T("bagscan.apply");
            TxtPickTitle.Text     = L10n.T("bagscan.pickTitle");
            BtnPickConfirm.Content= L10n.T("bagscan.confirm");
            BtnPickCancel.Content = L10n.T("bagscan.cancel");
            TxtResultsLabel.Text  = L10n.T("bagscan.results");
        }

        // ── Template list ──────────────────────────────────────────────

        private void RebuildTemplateList()
        {
            var rows = _vm.Settings.BagTemplates.Select(t => new TemplateRow
            {
                MaterialId = t.MaterialId,
                MatDisplay = _vm.GetMatIcon(t.MaterialId) + "  " + _vm.GetMatName(t.MaterialId),
                Preview    = t.Preview,
            }).ToList();

            TemplateList.ItemsSource    = rows;
            TxtNoTemplates.Visibility   = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            UpdateScanButton();
        }

        private void UpdateScanButton()
        {
            bool canScan = _screenshot != null && _vm?.Settings.BagTemplates.Count > 0;
            BtnScan.IsEnabled = canScan;
        }

        // ── Import screenshot ──────────────────────────────────────────

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title  = L10n.T("bagscan.import"),
                Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.webp|All files|*.*",
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource        = new Uri(dlg.FileName);
                bmp.CacheOption      = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();

                _screenshot = bmp;
                ImgScreenshot.Source = bmp;
                ImgScreenshot.Visibility  = Visibility.Visible;
                OverlayCanvas.Visibility  = Visibility.Visible;
                TxtNoScreenshot.Visibility = Visibility.Collapsed;

                DrawOverlay();
                UpdateScanButton();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Click on screenshot ────────────────────────────────────────

        private void ImgScreenshot_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_screenshot == null || _picking) return;

            var clickPt = e.GetPosition(ImgScreenshot);
            var imgCoords = DisplayToImage(ImgScreenshot, _screenshot, clickPt);
            if (imgCoords == null) return;

            _pendingX = imgCoords.Value.x;
            _pendingY = imgCoords.Value.y;

            // Build preview for the picker
            var previewPx = BagScanService.CreateTemplate(_screenshot, _pendingX, _pendingY);
            if (previewPx.Length > 0)
            {
                var wb = new WriteableBitmap(
                    BagScanService.TemplateSize, BagScanService.TemplateSize,
                    96, 96, PixelFormats.Bgr32, null);
                wb.WritePixels(
                    new Int32Rect(0, 0, BagScanService.TemplateSize, BagScanService.TemplateSize),
                    previewPx, BagScanService.TemplateSize * 4, 0);
                ImgPickPreview.Source = wb;
            }

            // Populate material list
            PickerList.ItemsSource = _vm.Materials.ToList();
            PickerList.SelectedIndex = 0;

            ShowPicker(true);
        }

        // ── Picker overlay ─────────────────────────────────────────────

        private void ShowPicker(bool show)
        {
            _picking = show;
            PickerOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnPickCancel_Click(object sender, RoutedEventArgs e)
            => ShowPicker(false);

        private void BtnPickConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (PickerList.SelectedItem is not Material mat)
            {
                MessageBox.Show("請先選擇一個材料。");
                return;
            }

            // Remove any existing template for this material
            _vm.Settings.BagTemplates.RemoveAll(t => t.MaterialId == mat.Id);

            // Create new template
            var px = BagScanService.CreateTemplate(_screenshot!, _pendingX, _pendingY);
            var tmpl = new BagTemplate
            {
                MaterialId = mat.Id,
                CenterX    = _pendingX,
                CenterY    = _pendingY,
            };
            tmpl.StorePixels(px);

            _vm.Settings.BagTemplates.Add(tmpl);
            _vm.UpdateSettings();

            ShowPicker(false);
            RebuildTemplateList();
            DrawOverlay();
        }

        // ── Delete template ────────────────────────────────────────────

        private void BtnDeleteTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string matId) return;
            _vm.Settings.BagTemplates.RemoveAll(t => t.MaterialId == matId);
            _vm.UpdateSettings();
            RebuildTemplateList();
            DrawOverlay();
        }

        // ── Cell size ──────────────────────────────────────────────────

        private void TxtCellSize_Changed(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(TxtCellSize.Text, out int sz) && sz >= 16 && sz <= 256)
            {
                _cellSize = sz;
                _vm?.Settings.Let(s => { s.BagCellSize = _cellSize; });
                _vm?.UpdateSettings();
            }
        }

        private void BtnCellSizeDec_Click(object sender, RoutedEventArgs e)
        {
            int sz = Math.Max(16, _cellSize - 4);
            TxtCellSize.Text = sz.ToString();
        }

        private void BtnCellSizeInc_Click(object sender, RoutedEventArgs e)
        {
            int sz = Math.Min(256, _cellSize + 4);
            TxtCellSize.Text = sz.ToString();
        }

        // ── Scan ───────────────────────────────────────────────────────

        private async void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            if (_screenshot == null || _vm.Settings.BagTemplates.Count == 0) return;

            BtnScan.IsEnabled  = false;
            BtnApply.IsEnabled = false;
            TxtScanStatus.Text = L10n.T("bagscan.scanning");

            bool fullScan = ChkFullScan.IsChecked == true;

            try
            {
                _lastResults = await BagScanService.ScanAsync(
                    _screenshot,
                    _vm.Settings.BagTemplates,
                    _cellSize,
                    fullScan,
                    id => _vm.GetMatName(id),
                    id => _vm.GetMatIcon(id));

                // Display results
                var rows = _lastResults.Select(r => new ResultRow
                {
                    MatIcon    = r.MatIcon,
                    MatName    = r.MatName,
                    Quantity   = r.Quantity,
                    Found      = r.Found,
                    MatchScore = r.MatchScore,
                }).ToList();

                ResultList.ItemsSource      = rows;
                SepResults.Visibility       = Visibility.Visible;
                TxtResultsLabel.Visibility  = Visibility.Visible;
                BtnApply.Visibility         = Visibility.Visible;

                int foundCount = _lastResults.Count(r => r.Found && r.Quantity >= 0);
                TxtScanStatus.Text  = $"✅ {foundCount} / {_lastResults.Count}";
                BtnApply.IsEnabled  = foundCount > 0;

                DrawScanOverlay();
            }
            catch (Exception ex)
            {
                TxtScanStatus.Text = "❌ " + ex.Message;
            }
            finally
            {
                BtnScan.IsEnabled = true;
            }
        }

        // ── Apply to inventory ─────────────────────────────────────────

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            int count = 0;
            foreach (var r in _lastResults)
            {
                if (r.Found && r.Quantity >= 0)
                {
                    _vm.SetMaterial(r.MaterialId, r.Quantity);
                    count++;
                }
            }
            TxtScanStatus.Text = L10n.T("bagscan.doneApplied",
                new() { ["n"] = count.ToString() });
        }

        // ── Overlay drawing ────────────────────────────────────────────

        private void DrawOverlay()
        {
            OverlayCanvas.Children.Clear();
            if (_screenshot == null || ImgScreenshot.ActualWidth <= 0) return;

            foreach (var tmpl in _vm.Settings.BagTemplates)
            {
                // Cell bounding box in image coords
                int cellX = tmpl.CenterX - _cellSize / 2;
                int cellY = tmpl.CenterY - _cellSize / 2;

                var (dx, dy, dw, dh) = ImageToDisplay(
                    ImgScreenshot, _screenshot,
                    cellX, cellY, _cellSize, _cellSize);

                // Green box
                var rect = new Rectangle
                {
                    Width           = dw,
                    Height          = dh,
                    Stroke          = Brushes.LimeGreen,
                    StrokeThickness = 2,
                };
                Canvas.SetLeft(rect, dx);
                Canvas.SetTop(rect,  dy);
                OverlayCanvas.Children.Add(rect);

                // Material name label
                var label = new TextBlock
                {
                    Text       = _vm.GetMatIcon(tmpl.MaterialId) + " " + _vm.GetMatName(tmpl.MaterialId),
                    FontSize   = 10,
                    Foreground = Brushes.LimeGreen,
                    Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                };
                Canvas.SetLeft(label, dx);
                Canvas.SetTop(label,  dy + dh);
                OverlayCanvas.Children.Add(label);
            }
        }

        /// <summary>
        /// Draw cyan boxes where templates were FOUND during last scan.
        /// Called after scan completes (in addition to the green registration boxes).
        /// </summary>
        private void DrawScanOverlay()
        {
            if (_screenshot == null || ImgScreenshot.ActualWidth <= 0) return;

            // Use cellSize so the cyan box aligns with the green registration box
            foreach (var r in _lastResults)
            {
                if (!r.Found) continue;

                // FoundX/Y is the template top-left (cx-20, cy-20).
                // Cell top-left = (cx - cellSize/2, cy - cellSize/2)
                //               = (FoundX + 20 - cellSize/2, FoundY + 20 - cellSize/2)
                int ts      = BagScanService.TemplateSize;
                int cellX   = r.FoundX + ts / 2 - _cellSize / 2;
                int cellY   = r.FoundY + ts / 2 - _cellSize / 2;

                var (dx, dy, dw, dh) = ImageToDisplay(
                    ImgScreenshot, _screenshot, cellX, cellY, _cellSize, _cellSize);

                // Cyan dashed box (on top of green registration box)
                var rect = new Rectangle
                {
                    Width           = dw,
                    Height          = dh,
                    Stroke          = Brushes.Cyan,
                    StrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection { 4, 3 },
                };
                Canvas.SetLeft(rect, dx);
                Canvas.SetTop(rect,  dy);
                OverlayCanvas.Children.Add(rect);

                // Score + quantity label above the box
                string qty = r.Quantity >= 0 ? r.Quantity.ToString("N0") : "?";
                var lbl = new TextBlock
                {
                    Text       = $"{qty} ({r.MatchScore:P0})",
                    FontSize   = 9,
                    Foreground = Brushes.Cyan,
                    Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                };
                Canvas.SetLeft(lbl, dx);
                Canvas.SetTop(lbl,  dy - 14);
                OverlayCanvas.Children.Add(lbl);
            }
        }

        private void ImgScreenshot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawOverlay();
            DrawScanOverlay();
        }

        // ── Coordinate helpers ─────────────────────────────────────────

        /// <summary>Convert display click position → image pixel coordinates.</summary>
        private static (int x, int y)? DisplayToImage(
            Image ctrl, BitmapSource bmp, Point clickPt)
        {
            if (ctrl.ActualWidth <= 0 || ctrl.ActualHeight <= 0) return null;

            double scaleX = ctrl.ActualWidth  / bmp.PixelWidth;
            double scaleY = ctrl.ActualHeight / bmp.PixelHeight;
            double scale  = Math.Min(scaleX, scaleY);

            double dispW  = bmp.PixelWidth  * scale;
            double dispH  = bmp.PixelHeight * scale;
            double offX   = (ctrl.ActualWidth  - dispW) / 2;
            double offY   = (ctrl.ActualHeight - dispH) / 2;

            double relX = clickPt.X - offX;
            double relY = clickPt.Y - offY;

            if (relX < 0 || relX >= dispW || relY < 0 || relY >= dispH) return null;

            return (
                Math.Clamp((int)(relX / scale), 0, bmp.PixelWidth  - 1),
                Math.Clamp((int)(relY / scale), 0, bmp.PixelHeight - 1));
        }

        /// <summary>Convert image-space rectangle → display-space coordinates.</summary>
        private static (double x, double y, double w, double h) ImageToDisplay(
            Image ctrl, BitmapSource bmp,
            int imgX, int imgY, int imgW, int imgH)
        {
            double scaleX = ctrl.ActualWidth  / bmp.PixelWidth;
            double scaleY = ctrl.ActualHeight / bmp.PixelHeight;
            double scale  = Math.Min(scaleX, scaleY);

            double offX = (ctrl.ActualWidth  - bmp.PixelWidth  * scale) / 2;
            double offY = (ctrl.ActualHeight - bmp.PixelHeight * scale) / 2;

            return (offX + imgX * scale, offY + imgY * scale, imgW * scale, imgH * scale);
        }
    }

    // ── Extension helper ────────────────────────────────────────────────
    internal static class SettingsExt
    {
        public static void Let<T>(this T obj, Action<T> action) => action(obj);
    }
}
