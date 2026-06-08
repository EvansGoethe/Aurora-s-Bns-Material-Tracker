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
        private int           _pendingX;     // image pixel coords of last click, waiting for material pick
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

                // Size the container to exact pixel dimensions.
                // The Viewbox then scales it uniformly to fit the panel.
                // This makes Canvas coordinates 1:1 with image pixels — no conversion needed.
                ImageContainer.Width  = bmp.PixelWidth;
                ImageContainer.Height = bmp.PixelHeight;

                VbScreenshot.Visibility    = Visibility.Visible;
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

            // Because ImageContainer is PixelWidth × PixelHeight and Stretch="Fill",
            // GetPosition(ImgScreenshot) returns pixel coordinates directly.
            var pt = e.GetPosition(ImgScreenshot);
            int px = Math.Clamp((int)pt.X, 0, _screenshot.PixelWidth  - 1);
            int py = Math.Clamp((int)pt.Y, 0, _screenshot.PixelHeight - 1);

            _pendingX = px;
            _pendingY = py;

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
                DrawOverlay();
                DrawScanOverlay();
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

                DrawOverlay();
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
        //
        // Because ImageContainer is sized to the screenshot's pixel dimensions and
        // the Image uses Stretch="Fill", the Canvas coordinate system is 1:1 with
        // image pixels.  No ImageToDisplay / DisplayToImage conversion is needed.

        private void DrawOverlay()
        {
            OverlayCanvas.Children.Clear();
            if (_screenshot == null) return;

            foreach (var tmpl in _vm.Settings.BagTemplates)
            {
                // Cell bounding box in IMAGE PIXEL coordinates (= Canvas coordinates)
                int cellX = tmpl.CenterX - _cellSize / 2;
                int cellY = tmpl.CenterY - _cellSize / 2;

                // Green registration box
                var rect = new Rectangle
                {
                    Width           = _cellSize,
                    Height          = _cellSize,
                    Stroke          = Brushes.LimeGreen,
                    StrokeThickness = 2,
                };
                Canvas.SetLeft(rect, cellX);
                Canvas.SetTop(rect,  cellY);
                OverlayCanvas.Children.Add(rect);

                // Material name label below the box
                var label = new TextBlock
                {
                    Text       = _vm.GetMatIcon(tmpl.MaterialId) + " " + _vm.GetMatName(tmpl.MaterialId),
                    FontSize   = 10,
                    Foreground = Brushes.LimeGreen,
                    Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                };
                Canvas.SetLeft(label, cellX);
                Canvas.SetTop(label,  cellY + _cellSize);
                OverlayCanvas.Children.Add(label);
            }
        }

        /// <summary>
        /// Draw cyan dashed boxes at the positions where templates were FOUND during
        /// the last scan.  Called after scan completes (green registration boxes remain).
        /// </summary>
        private void DrawScanOverlay()
        {
            if (_screenshot == null) return;

            foreach (var r in _lastResults)
            {
                if (!r.Found) continue;

                // FoundX/Y is template top-left (cx-20, cy-20 in pixel space).
                // Cell top-left = (cx - cellSize/2, cy - cellSize/2)
                //               = (FoundX + 20 - cellSize/2, FoundY + 20 - cellSize/2)
                int ts    = BagScanService.TemplateSize;
                int cellX = r.FoundX + ts / 2 - _cellSize / 2;
                int cellY = r.FoundY + ts / 2 - _cellSize / 2;

                // Cyan dashed box
                var rect = new Rectangle
                {
                    Width           = _cellSize,
                    Height          = _cellSize,
                    Stroke          = Brushes.Cyan,
                    StrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection { 4, 3 },
                };
                Canvas.SetLeft(rect, cellX);
                Canvas.SetTop(rect,  cellY);
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
                Canvas.SetLeft(lbl, cellX);
                Canvas.SetTop(lbl,  cellY - 14);
                OverlayCanvas.Children.Add(lbl);
            }
        }

        private void ImgScreenshot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // With Viewbox approach the canvas is in pixel space — no redraw needed on resize.
        }
    }

    // ── Extension helper ────────────────────────────────────────────────
    internal static class SettingsExt
    {
        public static void Let<T>(this T obj, Action<T> action) => action(obj);
    }
}
