using System.Windows;
using System.Windows.Controls;
using BnsMaterialTracker.Services;
using BnsMaterialTracker.ViewModels;
using BnsMaterialTracker.Views;

namespace BnsMaterialTracker
{
    public partial class MainWindow : Window
    {
        private AppViewModel _vm = null!;

        // ── One instance of each page ────────────────────
        private DashboardView    _dashboard   = null!;
        private DailyTrackerView _tracker     = null!;
        private CalculatorView   _calculator  = null!;
        private PredictorView    _predictor   = null!;
        private InventoryView    _inventory   = null!;
        private MarketView       _market      = null!;
        private DataEditorView   _dataEditor  = null!;
        private SettingsView     _settings    = null!;
        private BagScanView      _bagScan     = null!;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _vm = new AppViewModel();

            // Apply L10n to sidebar labels (and window title)
            ApplySidebarL10n();

            // Live-update sidebar + title when language changes
            _vm.LanguageChanged += () =>
            {
                ApplySidebarL10n();
                // Refresh current page so its labels also update immediately
                if (MainContent.Content is FrameworkElement fe &&
                    fe.GetType().GetMethod("Refresh") is { } m)
                    m.Invoke(fe, null);
            };

            // Create views (all share the same ViewModel)
            _dashboard   = new DashboardView   { DataContext = _vm };
            _tracker     = new DailyTrackerView { DataContext = _vm };
            _calculator  = new CalculatorView  { DataContext = _vm };
            _predictor   = new PredictorView   { DataContext = _vm };
            _inventory   = new InventoryView   { DataContext = _vm };
            _market      = new MarketView      { DataContext = _vm };
            _dataEditor  = new DataEditorView  { DataContext = _vm };
            _settings    = new SettingsView    { DataContext = _vm };
            _bagScan     = new BagScanView     { DataContext = _vm };

            ShowPage("dashboard");
        }

        private void ApplySidebarL10n()
        {
            Title              = L10n.T("app.windowTitle");
            TxtGameTitle.Text  = L10n.T("nav.gameTitle");
            TxtAppTitle.Text   = L10n.T("nav.appTitle");
            TxtServer.Text     = L10n.T("nav.server");
            TxtNavMain.Text    = L10n.T("nav.main");
            TxtNavAdmin.Text   = L10n.T("nav.admin");
            TxtOpenFolder.Text = "📂 " + L10n.T("nav.openFolder");

            // Nav labels
            SetNavText(NavDashboard,  "📊 " + L10n.T("nav.dashboard"));
            SetNavText(NavTracker,    "⚔️ " + L10n.T("nav.tracker"));
            SetNavText(NavCalculator, "🧮 " + L10n.T("nav.calculator"));
            SetNavText(NavPredictor,  "🎯 " + L10n.T("nav.predictor"));
            SetNavInventory(          "📦 " + L10n.T("nav.inventory"));
            SetNavText(NavMarket,     "💰 " + L10n.T("nav.market"));
            SetNavText(NavBagScan,    "🔍 " + L10n.T("nav.bagscan"));
            SetNavText(NavDataEditor, "✏️ " + L10n.T("nav.dataeditor"));
            SetNavText(NavSettings,   "⚙️ " + L10n.T("nav.settings"));
        }

        private static void SetNavText(RadioButton rb, string text)
        {
            if (rb.Content is TextBlock tb) tb.Text = text;
            else rb.Content = new TextBlock { Text = text };
        }

        private void SetNavInventory(string text)
        {
            if (NavInventory.Content is TextBlock tb) tb.Text = text;
            else NavInventory.Content = new TextBlock { Text = text };
        }

        private void Nav_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tag)
                ShowPage(tag);
        }

        private void ShowPage(string page)
        {
            switch (page)
            {
                case "dashboard":
                    _dashboard?.Refresh();
                    MainContent.Content = _dashboard;
                    break;
                case "tracker":
                    _tracker?.Refresh();
                    MainContent.Content = _tracker;
                    break;
                case "calculator":
                    _calculator?.Refresh();
                    MainContent.Content = _calculator;
                    break;
                case "predictor":
                    _predictor?.Refresh();
                    MainContent.Content = _predictor;
                    break;
                case "inventory":
                    _inventory?.Refresh();
                    MainContent.Content = _inventory;
                    break;
                case "market":
                    _market?.Refresh();
                    MainContent.Content = _market;
                    break;
                case "dataeditor":
                    _dataEditor?.Refresh();
                    MainContent.Content = _dataEditor;
                    break;
                case "settings":
                    _settings?.Refresh();
                    MainContent.Content = _settings;
                    break;
                case "bagscan":
                    _bagScan?.Refresh();
                    MainContent.Content = _bagScan;
                    break;
            }
        }

        private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
            => DataService.OpenDataFolder();
    }
}
