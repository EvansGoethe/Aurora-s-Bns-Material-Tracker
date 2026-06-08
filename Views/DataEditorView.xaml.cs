using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BnsMaterialTracker.Models;
using BnsMaterialTracker.Services;
using BnsMaterialTracker.ViewModels;
using Microsoft.Win32;

namespace BnsMaterialTracker.Views
{
    public partial class DataEditorView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n!));

        private AppViewModel _vm = null!;

        // ── Option item (Key stored in JSON, Label shown in UI) ─────────────
        public class CatOption
        {
            public string Key   { get; }
            public string Label { get; }
            public CatOption(string key, string label) { Key = key; Label = label; }
            public override string ToString() => Label;
        }

        // ── Emoji icon picker choices ───────────────────────────────────────
        private static readonly string[] _iconChoices =
        {
            "📦","💎","💠","🔮","🌟","⭐","✨","💫",
            "🔴","🔵","🔶","🔷","🔸","🔹","🔺","🔻",
            "🔥","❄️","⚡","🌀","💧","🌸","🍃","🌿",
            "⚔️","🛡️","🏹","💰","🎫","🔑","🗝️",
            "🧪","💍","📿","🏺","🎯","🏆","🎖️",
        };
        public string[] IconChoices => _iconChoices;

        // ── Option arrays (rebuilt on each Refresh so they follow language) ─
        private CatOption[] _dungeonTypeOptions     = Array.Empty<CatOption>();
        private CatOption[] _categoryOptions        = Array.Empty<CatOption>();
        private CatOption[] _upgradeCategoryOptions = Array.Empty<CatOption>();

        public CatOption[] DungeonTypeOptions
        {
            get => _dungeonTypeOptions;
            private set { _dungeonTypeOptions = value; OnPropChanged(); }
        }
        public CatOption[] CategoryOptions
        {
            get => _categoryOptions;
            private set { _categoryOptions = value; OnPropChanged(); }
        }
        public CatOption[] UpgradeCategoryOptions
        {
            get => _upgradeCategoryOptions;
            private set { _upgradeCategoryOptions = value; OnPropChanged(); }
        }

        private void RebuildOptions()
        {
            DungeonTypeOptions = new[] {
                new CatOption("daily",  L10n.T("editor.type.daily")),
                new CatOption("weekly", L10n.T("editor.type.weekly")),
            };
            var cats = new[] {
                new CatOption("weapon",        "⚔️ " + L10n.T("editor.cat.weapon")),
                new CatOption("bracelet",      "💠 " + L10n.T("calc.bracelet")),
                new CatOption("necklace",      "📿 " + L10n.T("calc.necklace")),
                new CatOption("belt",          "🔶 " + L10n.T("calc.belt")),
                new CatOption("gloves",        "🥊 " + L10n.T("calc.gloves")),
                new CatOption("earring",       "💎 " + L10n.T("calc.earring")),
                new CatOption("secretToken",   "🎴 " + L10n.T("calc.secretToken")),
                new CatOption("divineToken",   "🃏 " + L10n.T("calc.divineToken")),
                new CatOption("soul",          "👻 " + L10n.T("calc.soul")),
                new CatOption("spirit",        "✨ " + L10n.T("calc.spirit")),
                new CatOption("guardianStone", "🛡️ " + L10n.T("calc.guardianStone")),
                new CatOption("star",          "⭐ " + L10n.T("calc.star")),
                new CatOption("innerBracelet", "🔵 " + L10n.T("calc.innerBracelet")),
                new CatOption("outerBracelet", "🔴 " + L10n.T("calc.outerBracelet")),
                new CatOption("exchange",      "🎫 " + L10n.T("editor.cat.exchange")),
            };
            CategoryOptions = cats;
            // UpgradeCategoryOptions is the same list minus "exchange"
            UpgradeCategoryOptions = cats[..^1];

            // Re-select upgrade category by key so the filter survives a language switch
            if (_selectedUpgradeCat != null)
            {
                var prevKey = _selectedUpgradeCat.Key;
                _selectedUpgradeCat = UpgradeCategoryOptions.FirstOrDefault(o => o.Key == prevKey)
                                      ?? UpgradeCategoryOptions[0];
                OnPropChanged(nameof(SelectedUpgradeCat));
            }
        }

        // ── Observable collections ──────────────────────────────────────────
        public ObservableCollection<DungeonRow>     DungeonList      { get; } = new();
        public ObservableCollection<MaterialRow>    MaterialList     { get; } = new();
        public ObservableCollection<UpgradeStepRow> UpgradeList      { get; } = new();
        public ObservableCollection<UpgradeStepRow> FilteredUpgrades { get; } = new();

        private CatOption? _selectedUpgradeCat;
        public CatOption? SelectedUpgradeCat
        {
            get => _selectedUpgradeCat;
            set { _selectedUpgradeCat = value; OnPropChanged(); FilterUpgrades(); }
        }

        // ── Row model classes ───────────────────────────────────────────────
        public abstract class NotifyBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnProp([CallerMemberName] string? n = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n!));
        }

        public class DropRow : NotifyBase
        {
            private string _materialId = "";
            private double _chance = 1.0;
            private int _min = 1, _max = 1;
            public string MaterialId { get => _materialId; set { _materialId = value; OnProp(); } }
            public double Chance     { get => _chance;     set { _chance = value;     OnProp(); } }
            public int    Min        { get => _min;        set { _min = value;         OnProp(); } }
            public int    Max        { get => _max;        set { _max = value;         OnProp(); } }
            public DungeonRow? Parent { get; set; }
        }

        public class DungeonRow : NotifyBase
        {
            private string _id = "", _name = "", _shortName = "", _type = "daily";
            private string _mode = "hero", _difficulty = "normal";
            private int _dailyLimit = 5, _weeklyLimit = 3, _estimatedMinutes = 20;

            public string Id               { get => _id;               set { _id = value;               OnProp(); } }
            public string Name             { get => _name;             set { _name = value;             OnProp(); } }
            public string ShortName        { get => _shortName;        set { _shortName = value;        OnProp(); } }
            public string Type             { get => _type;             set { _type = value;             OnProp(); } }
            public int    DailyLimit       { get => _dailyLimit;       set { _dailyLimit = value;       OnProp(); } }
            public int    WeeklyLimit      { get => _weeklyLimit;      set { _weeklyLimit = value;      OnProp(); } }
            public int    EstimatedMinutes { get => _estimatedMinutes; set { _estimatedMinutes = value; OnProp(); } }

            public string Mode
            {
                get => _mode;
                set
                {
                    if (_mode == value) return;
                    _mode = value;
                    OnProp();
                    // Reset difficulty to first valid value for the new mode
                    _difficulty = value == "demon" ? "1" : "easy";
                    OnProp(nameof(Difficulty));
                    OnProp(nameof(DifficultyOptions));
                }
            }

            public string Difficulty
            {
                get => _difficulty;
                set { _difficulty = value; OnProp(); }
            }

            // Options dynamically derived from current Mode — used directly in XAML
            public CatOption[] ModeOptions => new[]
            {
                new CatOption("hero",  L10n.T("editor.mode.hero")),
                new CatOption("demon", L10n.T("editor.mode.demon")),
            };

            public CatOption[] DifficultyOptions
            {
                get
                {
                    if (_mode == "demon")
                        return System.Linq.Enumerable.Range(1, 7)
                            .Select(i => new CatOption(i.ToString(),
                                i + L10n.T("editor.demon.seg")))
                            .ToArray();
                    return new[]
                    {
                        new CatOption("easy",    L10n.T("editor.difficulty.easy")),
                        new CatOption("normal",  L10n.T("editor.difficulty.normal")),
                        new CatOption("skilled", L10n.T("editor.difficulty.skilled")),
                    };
                }
            }

            public ObservableCollection<DropRow> Drops { get; } = new();
        }

        public class MaterialRow : NotifyBase
        {
            private string _id = "", _name = "", _icon = "📦", _category = "weapon";
            public string Id       { get => _id;       set { _id = value;       OnProp(); } }
            public string Name     { get => _name;     set { _name = value;     OnProp(); } }
            public string Icon     { get => _icon;     set { _icon = value;     OnProp(); } }
            public string Category { get => _category; set { _category = value; OnProp(); } }
        }

        public class ReqRow : NotifyBase
        {
            private string _materialId = "";
            private int _amount = 1;
            public string MaterialId { get => _materialId; set { _materialId = value; OnProp(); } }
            public int    Amount     { get => _amount;     set { _amount = value;     OnProp(); } }
            public UpgradeStepRow? Parent { get; set; }
        }

        public class UpgradeStepRow : NotifyBase
        {
            private string _id = "", _name = "", _category = "weapon";
            private int _fromStage = 1, _toStage = 2, _goldCost = 0;
            public string Id        { get => _id;        set { _id = value;        OnProp(); } }
            public string Name      { get => _name;      set { _name = value;      OnProp(); } }
            public string Category  { get => _category;  set { _category = value;  OnProp(); } }
            public int    FromStage { get => _fromStage; set { _fromStage = value; OnProp(); } }
            public int    ToStage   { get => _toStage;   set { _toStage = value;   OnProp(); } }
            public int    GoldCost  { get => _goldCost;  set { _goldCost = value;  OnProp(); } }
            public ObservableCollection<ReqRow> Requirements { get; } = new();
        }

        // ── Constructor & Refresh ───────────────────────────────────────────
        public DataEditorView() => InitializeComponent();

        public void Refresh()
        {
            if (DataContext is AppViewModel vm) { _vm = vm; DataContext = this; }

            TxtTitle.Text        = L10n.T("editor.title");
            TxtSubtitle.Text     = L10n.T("editor.subtitle");
            TxtTabDungeons.Text  = L10n.T("editor.dungeons");
            TxtTabMaterials.Text = L10n.T("editor.materials");
            TxtTabUpgrades.Text  = L10n.T("editor.upgrades");

            RebuildOptions();
            LoadAllData();
            if (SelectedUpgradeCat == null)
                SelectedUpgradeCat = UpgradeCategoryOptions[0];
            else
                FilterUpgrades();

            TabDungeons.IsChecked = true;
            ShowPanel("dungeons");
            TxtStatus.Text = "";
        }

        // ── Load ────────────────────────────────────────────────────────────
        private void LoadAllData()
        {
            LoadDungeons();
            LoadMaterials();
            LoadUpgrades();
        }

        private void LoadDungeons()
        {
            DungeonList.Clear();
            var path = Path.Combine(DataService.GameDataDir, "dungeons.json");
            if (!File.Exists(path)) return;
            try
            {
                var file = JsonSerializer.Deserialize<DungeonsFile>(File.ReadAllText(path));
                if (file == null) return;
                foreach (var d in file.Dungeons)
                {
                    var row = new DungeonRow
                    {
                        Id = d.Id, Name = d.Name, ShortName = d.ShortName,
                        Type = d.Type, Mode = d.Mode, Difficulty = d.Difficulty,
                        DailyLimit = d.DailyLimit ?? 5,
                        WeeklyLimit = d.WeeklyLimit ?? 3,
                        EstimatedMinutes = d.EstimatedMinutes,
                    };
                    foreach (var dr in d.Drops)
                        row.Drops.Add(new DropRow { MaterialId = dr.MaterialId, Chance = dr.Chance,
                            Min = dr.Min, Max = dr.Max, Parent = row });
                    DungeonList.Add(row);
                }
            }
            catch { /* ignore parse errors on load */ }
        }

        private void LoadMaterials()
        {
            MaterialList.Clear();
            var path = Path.Combine(DataService.GameDataDir, "materials.json");
            if (!File.Exists(path)) return;
            try
            {
                var file = JsonSerializer.Deserialize<MaterialsFile>(File.ReadAllText(path));
                if (file == null) return;
                foreach (var m in file.Materials)
                    MaterialList.Add(new MaterialRow { Id = m.Id, Name = m.Name, Icon = m.Icon, Category = m.Category });
            }
            catch { }
        }

        private void LoadUpgrades()
        {
            UpgradeList.Clear();
            var path = Path.Combine(DataService.GameDataDir, "upgrades.json");
            if (!File.Exists(path)) return;
            try
            {
                var file = JsonSerializer.Deserialize<UpgradesFile>(File.ReadAllText(path));
                if (file == null) return;

                void AddSteps(System.Collections.Generic.IEnumerable<UpgradeStep> steps)
                {
                    foreach (var s in steps)
                    {
                        var row = new UpgradeStepRow { Id = s.Id, Name = s.Name, Category = s.Category,
                            FromStage = s.FromStage, ToStage = s.ToStage, GoldCost = s.GoldCost };
                        foreach (var r in s.Requirements)
                            row.Requirements.Add(new ReqRow { MaterialId = r.MaterialId, Amount = r.Amount, Parent = row });
                        UpgradeList.Add(row);
                    }
                }

                AddSteps(file.WeaponUpgrades);        AddSteps(file.BraceletUpgrades);
                AddSteps(file.NecklaceUpgrades);      AddSteps(file.BeltUpgrades);
                AddSteps(file.GlovesUpgrades);        AddSteps(file.EarringUpgrades);
                AddSteps(file.SecretTokenUpgrades);   AddSteps(file.DivineTokenUpgrades);
                AddSteps(file.SoulUpgrades);          AddSteps(file.SpiritUpgrades);
                AddSteps(file.GuardianStoneUpgrades); AddSteps(file.StarUpgrades);
                AddSteps(file.InnerBraceletUpgrades); AddSteps(file.OuterBraceletUpgrades);
            }
            catch { }
        }

        private void FilterUpgrades()
        {
            FilteredUpgrades.Clear();
            if (_selectedUpgradeCat == null) return;
            foreach (var s in UpgradeList.Where(u => u.Category == _selectedUpgradeCat.Key))
                FilteredUpgrades.Add(s);
        }

        // ── Panel visibility ────────────────────────────────────────────────
        private void ShowPanel(string tab)
        {
            if (PanelDungeons == null) return;
            PanelDungeons.Visibility  = tab == "dungeons"  ? Visibility.Visible : Visibility.Collapsed;
            PanelMaterials.Visibility = tab == "materials" ? Visibility.Visible : Visibility.Collapsed;
            PanelUpgrades.Visibility  = tab == "upgrades"  ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tag)
                ShowPanel(tag);
        }

        // ── Add ─────────────────────────────────────────────────────────────
        private void AddDungeon_Click(object sender, RoutedEventArgs e)
            => DungeonList.Add(new DungeonRow { Id = $"dungeon_{DungeonList.Count + 1}", Name = "新副本" });

        private void AddDrop_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is DungeonRow d)
                d.Drops.Add(new DropRow { MaterialId = "", Chance = 1.0, Min = 1, Max = 1, Parent = d });
        }

        private void AddMaterial_Click(object sender, RoutedEventArgs e)
            => MaterialList.Add(new MaterialRow { Id = $"mat_{MaterialList.Count + 1}", Name = "新材料" });

        private void AddUpgradeStep_Click(object sender, RoutedEventArgs e)
        {
            string cat = SelectedUpgradeCat?.Key ?? "weapon";
            int idx = UpgradeList.Count(u => u.Category == cat) + 1;
            var row = new UpgradeStepRow { Id = $"{cat}_{idx}", Name = "新升級步驟", Category = cat };
            UpgradeList.Add(row);
            FilteredUpgrades.Add(row);
        }

        private void AddReq_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is UpgradeStepRow s)
                s.Requirements.Add(new ReqRow { MaterialId = "", Amount = 1, Parent = s });
        }

        // ── Delete ──────────────────────────────────────────────────────────
        private void DeleteDungeon_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is DungeonRow r) DungeonList.Remove(r);
        }

        private void DeleteDrop_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is DropRow r) r.Parent?.Drops.Remove(r);
        }

        private void DeleteMaterial_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is MaterialRow r) MaterialList.Remove(r);
        }

        private void DeleteUpgradeStep_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is UpgradeStepRow r)
            {
                UpgradeList.Remove(r);
                FilteredUpgrades.Remove(r);
            }
        }

        private void DeleteReq_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is ReqRow r) r.Parent?.Requirements.Remove(r);
        }

        // ── Save ────────────────────────────────────────────────────────────
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveDungeons();
                SaveMaterials();
                SaveUpgrades();
                _vm?.ReloadGameData();
                TxtStatus.Text = $"✓ 已儲存全部 — {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"❌ 錯誤: {ex.Message}";
            }
        }

        private static readonly JsonSerializerOptions _jsonOpts = new() { WriteIndented = true };

        private void SaveDungeons()
        {
            var list = DungeonList.Select(d => new Dungeon
            {
                Id = d.Id, Name = d.Name, ShortName = d.ShortName,
                Type = d.Type, Mode = d.Mode, Difficulty = d.Difficulty,
                DailyLimit  = d.Type == "daily"  ? d.DailyLimit  : (int?)null,
                WeeklyLimit = d.Type == "weekly" ? d.WeeklyLimit : (int?)null,
                EstimatedMinutes = d.EstimatedMinutes,
                Drops = d.Drops.Select(dr => new MaterialDrop
                    { MaterialId = dr.MaterialId, Chance = dr.Chance, Min = dr.Min, Max = dr.Max }).ToList(),
            }).ToList();
            var path = Path.Combine(DataService.GameDataDir, "dungeons.json");
            File.WriteAllText(path, JsonSerializer.Serialize(new DungeonsFile { Dungeons = list }, _jsonOpts));
        }

        private void SaveMaterials()
        {
            var list = MaterialList.Select(m => new Material
                { Id = m.Id, Name = m.Name, Icon = m.Icon, Category = m.Category }).ToList();
            var path = Path.Combine(DataService.GameDataDir, "materials.json");
            File.WriteAllText(path, JsonSerializer.Serialize(new MaterialsFile { Materials = list }, _jsonOpts));
        }

        private void SaveUpgrades()
        {
            System.Collections.Generic.List<UpgradeStep> ToSteps(string cat) =>
                UpgradeList.Where(u => u.Category == cat).Select(s => new UpgradeStep
                {
                    Id = s.Id, Name = s.Name, Category = s.Category,
                    FromStage = s.FromStage, ToStage = s.ToStage, GoldCost = s.GoldCost,
                    Requirements = s.Requirements.Select(r => new UpgradeRequirement
                        { MaterialId = r.MaterialId, Amount = r.Amount }).ToList(),
                }).ToList();

            var file = new UpgradesFile
            {
                WeaponUpgrades        = ToSteps("weapon"),
                BraceletUpgrades      = ToSteps("bracelet"),
                NecklaceUpgrades      = ToSteps("necklace"),
                BeltUpgrades          = ToSteps("belt"),
                GlovesUpgrades        = ToSteps("gloves"),
                EarringUpgrades       = ToSteps("earring"),
                SecretTokenUpgrades   = ToSteps("secretToken"),
                DivineTokenUpgrades   = ToSteps("divineToken"),
                SoulUpgrades          = ToSteps("soul"),
                SpiritUpgrades        = ToSteps("spirit"),
                GuardianStoneUpgrades = ToSteps("guardianStone"),
                StarUpgrades          = ToSteps("star"),
                InnerBraceletUpgrades = ToSteps("innerBracelet"),
                OuterBraceletUpgrades = ToSteps("outerBracelet"),
            };
            var path = Path.Combine(DataService.GameDataDir, "upgrades.json");
            File.WriteAllText(path, JsonSerializer.Serialize(file, _jsonOpts));
        }

        private void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            LoadAllData();
            FilterUpgrades();
            TxtStatus.Text = "↺ 已重新載入";
        }

        // ── Dungeon import from screenshot ──────────────────────────────────

        private DungeonScanResult? _dungScanResult;

        private void ShowDungImport(bool show)
        {
            DungImportOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            if (!show) _dungScanResult = null;
        }

        private void BtnFromScreenshot_Click(object sender, RoutedEventArgs e)
        {
            DungResultPanel.Visibility  = Visibility.Collapsed;
            TxtDungStatus.Text          = "請選擇或貼上副本資訊頁的截圖";
            BtnDungConfirm.IsEnabled    = false;
            _dungScanResult             = null;
            ShowDungImport(true);
        }

        private void BtnDungCancel_Click(object sender, RoutedEventArgs e)
            => ShowDungImport(false);

        private void BtnDungImport_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title  = "選擇副本截圖",
                Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp|All files|*.*",
            };
            if (dlg.ShowDialog() != true) return;
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource   = new Uri(dlg.FileName);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                _ = RunDungeonScanAsync(bmp);
            }
            catch (Exception ex) { TxtDungStatus.Text = "❌ " + ex.Message; }
        }

        private void BtnDungPaste_Click(object sender, RoutedEventArgs e)
        {
            if (!Clipboard.ContainsImage()) { TxtDungStatus.Text = "剪貼簿中沒有圖片"; return; }
            var bmp = Clipboard.GetImage();
            if (bmp == null) { TxtDungStatus.Text = "無法讀取剪貼簿圖片"; return; }
            _ = RunDungeonScanAsync(bmp);
        }

        private async Task RunDungeonScanAsync(BitmapSource screenshot)
        {
            TxtDungStatus.Text         = "🔍 辨識中...";
            DungResultPanel.Visibility = Visibility.Collapsed;
            BtnDungConfirm.IsEnabled   = false;

            try { _dungScanResult = await DungeonScanService.ScanAsync(screenshot); }
            catch (Exception ex) { TxtDungStatus.Text = "❌ " + ex.Message; return; }

            if (_dungScanResult == null)
            {
                TxtDungStatus.Text = "❌ OCR 引擎不可用";
                return;
            }

            TxtDungName.Text = _dungScanResult.DungeonName;

            // Show raw OCR text for debugging
            TxtOcrRaw.Text          = _dungScanResult.RawOcrText;
            ExpanderOcr.Visibility  = Visibility.Visible;

            var entries = BuildPreviewEntries(_dungScanResult);
            DungEntryList.ItemsSource = entries;

            int total = _dungScanResult.Sections.Sum(s => s.ItemNames.Count);
            int unmatched = CountUnmatched(_dungScanResult);

            TxtDungStatus.Text = $"✅ 偵測完成 — 找到 {total} 個掉落名稱";

            if (unmatched > 0)
            {
                TxtDungUnmatched.Text       = $"⚠️ {unmatched} 個掉落名稱未能匹配現有材料，匯入後 materialId 欄位留空，請手動補全";
                TxtDungUnmatched.Visibility = Visibility.Visible;
            }
            else
            {
                TxtDungUnmatched.Visibility = Visibility.Collapsed;
            }

            DungResultPanel.Visibility = Visibility.Visible;
            BtnDungConfirm.IsEnabled   = entries.Count > 0;
        }

        private List<string> BuildPreviewEntries(DungeonScanResult result)
        {
            var entries  = new List<string>();
            int common   = result.Sections.FirstOrDefault(s => s.Difficulty == "common")?.ItemNames.Count ?? 0;

            if (result.Mode == "hero")
            {
                foreach (var (diff, label) in new[] { ("easy","入門"), ("normal","一般"), ("skilled","熟練") })
                {
                    var sec   = result.Sections.FirstOrDefault(s => s.Difficulty == diff);
                    int count = (sec?.ItemNames.Count ?? 0) + common;
                    if (sec != null || common > 0)
                        entries.Add($"  • {result.DungeonName}（{label}） — {count} 個掉落");
                }
            }
            else
            {
                for (int i = 1; i <= 7; i++)
                {
                    var sec = result.Sections.FirstOrDefault(s => s.Difficulty == i.ToString());
                    if (sec == null) continue;
                    entries.Add($"  • {result.DungeonName}（封魔{i}段） — {sec.ItemNames.Count + common} 個掉落");
                }
            }

            if (entries.Count == 0 && common > 0)
                entries.Add($"  • {result.DungeonName} — {common} 個掉落");

            return entries;
        }

        private int CountUnmatched(DungeonScanResult result)
            => result.Sections
                     .SelectMany(s => s.ItemNames)
                     .Distinct()
                     .Count(name => string.IsNullOrEmpty(MatchMaterialId(name)));

        private void BtnDungConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (_dungScanResult == null) return;

            string baseName = TxtDungName.Text.Trim();
            if (string.IsNullOrEmpty(baseName)) baseName = _dungScanResult.DungeonName;

            var common     = _dungScanResult.Sections.FirstOrDefault(s => s.Difficulty == "common");
            var commonItems = common?.ItemNames ?? new List<string>();
            int added      = 0;

            if (_dungScanResult.Mode == "hero")
            {
                foreach (var (diff, label) in new[] { ("easy","入門"), ("normal","一般"), ("skilled","熟練") })
                {
                    var sec = _dungScanResult.Sections.FirstOrDefault(s => s.Difficulty == diff);
                    if (sec == null && commonItems.Count == 0) continue;
                    var row = BuildDungeonRow(baseName, label, "hero", diff, commonItems, sec?.ItemNames ?? new List<string>(), added);
                    DungeonList.Add(row);
                    added++;
                }
            }
            else
            {
                for (int i = 1; i <= 7; i++)
                {
                    var sec = _dungScanResult.Sections.FirstOrDefault(s => s.Difficulty == i.ToString());
                    if (sec == null) continue;
                    var row = BuildDungeonRow(baseName, $"封魔{i}段", "demon", i.ToString(), commonItems, sec.ItemNames, added);
                    DungeonList.Add(row);
                    added++;
                }
            }

            TxtStatus.Text = $"✓ 從截圖匯入 {added} 個副本條目，請確認掉落後儲存";
            ShowDungImport(false);
            TabDungeons.IsChecked = true;
            ShowPanel("dungeons");
        }

        private DungeonRow BuildDungeonRow(
            string baseName, string diffLabel, string mode, string diff,
            List<string> commonItems, List<string> diffItems, int index)
        {
            string fullName = mode == "hero" || diffItems.Count > 0
                ? $"{baseName}（{diffLabel}）"
                : baseName;

            var row = new DungeonRow
            {
                Id               = $"{SanitizeId(baseName)}_{diff}",
                Name             = fullName,
                ShortName        = "",
                Mode             = mode,
                Difficulty       = diff,
                Type             = "daily",
                DailyLimit       = 5,
                EstimatedMinutes = 20,
            };

            foreach (var item in commonItems.Concat(diffItems))
                row.Drops.Add(new DropRow
                {
                    MaterialId = MatchMaterialId(item),
                    Chance     = 1.0,
                    Min        = 1,
                    Max        = 1,
                    Parent     = row,
                });

            return row;
        }

        private static string SanitizeId(string name)
            => Regex.Replace(name, @"[^\w一-鿿]", "_").ToLowerInvariant();

        private string MatchMaterialId(string itemName)
        {
            var exact = MaterialList.FirstOrDefault(m => m.Name == itemName);
            if (exact != null) return exact.Id;

            var partial = MaterialList.FirstOrDefault(m =>
                m.Name.Contains(itemName) || itemName.Contains(m.Name));
            return partial?.Id ?? "";
        }
    }
}
