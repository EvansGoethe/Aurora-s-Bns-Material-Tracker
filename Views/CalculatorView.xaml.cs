using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BnsMaterialTracker.Services;
using BnsMaterialTracker.ViewModels;

namespace BnsMaterialTracker.Views
{
    public partial class CalculatorView : UserControl
    {
        private AppViewModel _vm = null!;

        public class UpgradeItem : INotifyPropertyChanged
        {
            public string Id    { get; set; } = "";
            public string Label { get; set; } = "";
            private bool _checked;
            public bool Checked
            {
                get => _checked;
                set { _checked = value; PropertyChanged?.Invoke(this, new(nameof(Checked))); }
            }
            public event PropertyChangedEventHandler? PropertyChanged;
        }

        // 每個分類一個 List
        private readonly List<UpgradeItem> _weapon        = new();
        private readonly List<UpgradeItem> _bracelet      = new();
        private readonly List<UpgradeItem> _necklace      = new();
        private readonly List<UpgradeItem> _belt          = new();
        private readonly List<UpgradeItem> _gloves        = new();
        private readonly List<UpgradeItem> _earring       = new();
        private readonly List<UpgradeItem> _secretToken   = new();
        private readonly List<UpgradeItem> _divineToken   = new();
        private readonly List<UpgradeItem> _soul          = new();
        private readonly List<UpgradeItem> _spirit        = new();
        private readonly List<UpgradeItem> _guardianStone = new();
        private readonly List<UpgradeItem> _star          = new();
        private readonly List<UpgradeItem> _innerBracelet    = new();
        private readonly List<UpgradeItem> _outerBracelet    = new();
        private readonly List<UpgradeItem> _waterMeteor      = new();
        private readonly List<UpgradeItem> _woodMeteor       = new();
        private readonly List<UpgradeItem> _fireMeteor       = new();
        private readonly List<UpgradeItem> _earthMeteor      = new();
        private readonly List<UpgradeItem> _lightningMeteor  = new();

        public CalculatorView() => InitializeComponent();

        public void Refresh()
        {
            if (DataContext is not AppViewModel vm) return;
            _vm = vm;

            // Labels
            TxtTitle.Text         = L10n.T("calc.title");
            TxtWeaponLabel.Text   = "⚔️ " + L10n.T("calc.weapon");
            TxtAccLabel.Text      = "💍 " + L10n.T("calc.accessory");
            TxtBracelet.Text      = L10n.T("calc.bracelet");
            TxtNecklace.Text      = L10n.T("calc.necklace");
            TxtBelt.Text          = L10n.T("calc.belt");
            TxtGloves.Text        = L10n.T("calc.gloves");
            TxtEarring.Text       = L10n.T("calc.earring");
            TxtSecretToken.Text   = "🎴 " + L10n.T("calc.secretToken");
            TxtDivineToken.Text   = "🃏 " + L10n.T("calc.divineToken");
            TxtSoul.Text          = "👻 " + L10n.T("calc.soul");
            TxtSpirit.Text        = "✨ " + L10n.T("calc.spirit");
            TxtGuardianStone.Text = "🛡️ " + L10n.T("calc.guardianStone");
            TxtStar.Text          = "⭐ " + L10n.T("calc.star");
            TxtInnerBracelet.Text  = "🔵 " + L10n.T("calc.innerBracelet");
            TxtOuterBracelet.Text  = "🔴 " + L10n.T("calc.outerBracelet");
            TxtMeteoriteLabel.Text = "☄️ " + L10n.T("calc.meteorite");
            TxtWaterMeteor.Text    = "💧 " + L10n.T("calc.waterMeteor");
            TxtWoodMeteor.Text     = "🌿 " + L10n.T("calc.woodMeteor");
            TxtFireMeteor.Text     = "🔥 " + L10n.T("calc.fireMeteor");
            TxtEarthMeteor.Text    = "⛰️ " + L10n.T("calc.earthMeteor");
            TxtLightningMeteor.Text= "⚡ " + L10n.T("calc.lightningMeteor");
            TxtSaveGoal.Text      = L10n.T("calc.saveGoal");
            TxtTotalRequired.Text = L10n.T("calc.totalRequired");
            TxtShortfalls.Text    = L10n.T("calc.shortfalls");
            TxtSufficient.Text    = L10n.T("calc.sufficient");
            TxtSelectHint.Text    = L10n.T("calc.selectHint");

            // Rebuild lists
            Rebuild(_weapon,        vm, "weapon",        WeaponList);
            Rebuild(_bracelet,      vm, "bracelet",      BraceletList);
            Rebuild(_necklace,      vm, "necklace",      NecklaceList);
            Rebuild(_belt,          vm, "belt",          BeltList);
            Rebuild(_gloves,        vm, "gloves",        GlovesList);
            Rebuild(_earring,       vm, "earring",       EarringList);
            Rebuild(_secretToken,   vm, "secretToken",   SecretTokenList);
            Rebuild(_divineToken,   vm, "divineToken",   DivineTokenList);
            Rebuild(_soul,          vm, "soul",          SoulList);
            Rebuild(_spirit,        vm, "spirit",        SpiritList);
            Rebuild(_guardianStone, vm, "guardianStone", GuardianStoneList);
            Rebuild(_star,          vm, "star",          StarList);
            Rebuild(_innerBracelet,   vm, "innerBracelet",    InnerBraceletList);
            Rebuild(_outerBracelet,   vm, "outerBracelet",    OuterBraceletList);
            Rebuild(_waterMeteor,     vm, "waterMeteor",      WaterMeteorList);
            Rebuild(_woodMeteor,      vm, "woodMeteor",       WoodMeteorList);
            Rebuild(_fireMeteor,      vm, "fireMeteor",       FireMeteorList);
            Rebuild(_earthMeteor,     vm, "earthMeteor",      EarthMeteorList);
            Rebuild(_lightningMeteor, vm, "lightningMeteor",  LightningMeteorList);

            UpdateResults();
        }

        private static void Rebuild(List<UpgradeItem> items, AppViewModel vm,
            string category, ItemsControl ctrl)
        {
            var kept = items.Where(x => x.Checked).Select(x => x.Id).ToHashSet();
            items.Clear();
            foreach (var s in vm.AllUpgrades.Where(u => u.Category == category))
                items.Add(new UpgradeItem { Id = s.Id, Label = s.Name, Checked = kept.Contains(s.Id) });
            ctrl.ItemsSource = null;
            ctrl.ItemsSource = items;
        }

        // 所有分類合並
        private IEnumerable<UpgradeItem> AllItems()
            => _weapon.Concat(_bracelet).Concat(_necklace).Concat(_belt)
               .Concat(_gloves).Concat(_earring).Concat(_secretToken)
               .Concat(_divineToken).Concat(_soul).Concat(_spirit)
               .Concat(_guardianStone).Concat(_star)
               .Concat(_innerBracelet).Concat(_outerBracelet)
               .Concat(_waterMeteor).Concat(_woodMeteor).Concat(_fireMeteor)
               .Concat(_earthMeteor).Concat(_lightningMeteor);

        private void ChkChanged(object sender, RoutedEventArgs e) => UpdateResults();

        private void UpdateResults()
        {
            if (_vm == null) return;
            var selected = AllItems().Where(x => x.Checked).Select(x => x.Id).ToList();

            if (selected.Count == 0)
            {
                TxtSelectHint.Visibility = Visibility.Visible;
                CardRequired.Visibility  = Visibility.Collapsed;
                CardShortfall.Visibility = Visibility.Collapsed;
                return;
            }
            TxtSelectHint.Visibility = Visibility.Collapsed;
            CardRequired.Visibility  = Visibility.Visible;
            CardShortfall.Visibility = Visibility.Visible;

            var required = _vm.CalcRequired(selected);
            int goldCost = _vm.CalcTotalGold(selected);
            TxtGoldCost.Text = goldCost > 0 ? $"💰 {goldCost} " + L10n.T("common.gold") : "";

            var green = new SolidColorBrush(Color.FromRgb(34, 197, 94));
            var red   = new SolidColorBrush(Color.FromRgb(239, 68, 68));

            RequiredList.ItemsSource = required.Select(kv =>
            {
                int have = _vm.GetMaterial(kv.Key);
                bool ok  = have >= kv.Value;
                return new
                {
                    Icon      = _vm.GetMatIcon(kv.Key),
                    Name      = _vm.GetMatName(kv.Key),
                    HaveLabel = L10n.T("calc.have") + ": " + have,
                    NeedLabel = L10n.T("calc.need") + ": " + kv.Value,
                    SuffColor = ok ? green : red,
                };
            }).ToList();

            var shortfalls = required.Where(kv => _vm.GetMaterial(kv.Key) < kv.Value).ToList();
            if (shortfalls.Count == 0)
            {
                TxtSufficient.Visibility = Visibility.Visible;
                ShortfallList.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtSufficient.Visibility = Visibility.Collapsed;
                ShortfallList.Visibility = Visibility.Visible;
                ShortfallList.ItemsSource = shortfalls.Select(kv =>
                {
                    int shortage = kv.Value - _vm.GetMaterial(kv.Key);
                    var runs     = new List<string>();
                    bool hasAny  = false;
                    foreach (var d in _vm.Dungeons)
                    {
                        var drop = d.Drops.FirstOrDefault(dr => dr.MaterialId == kv.Key);
                        if (drop == null) continue;
                        double avg = drop.Chance * ((drop.Min + drop.Max) / 2.0);
                        if (avg <= 0) continue;
                        hasAny = true;
                        int n = (int)System.Math.Ceiling(shortage / avg);
                        runs.Add(L10n.T("calc.dungeonRuns", new() { ["name"] = d.Name, ["n"] = n }));
                    }
                    string noSrc = hasAny ? "" : L10n.T("calc.noSource");
                    return new
                    {
                        MatLine    = $"{_vm.GetMatIcon(kv.Key)} {_vm.GetMatName(kv.Key)} — " +
                                     L10n.T("calc.shortage", new() { ["n"] = shortage }),
                        Runs       = runs,
                        NoSource   = noSrc,
                        NoSourceVis= noSrc.Length > 0 ? Visibility.Visible : Visibility.Collapsed,
                    };
                }).ToList();
            }
        }

        private void BtnSaveGoal_Click(object sender, RoutedEventArgs e)
        {
            if (_vm == null) return;
            var selected = AllItems().Where(x => x.Checked).Select(x => x.Id).ToList();
            if (selected.Count == 0) return;
            string name = TxtGoalName.Text.Trim();
            if (string.IsNullOrEmpty(name)) name = L10n.T("common.unnamed");
            _vm.AddGoal(new BnsMaterialTracker.Models.Goal
            {
                Id         = System.Guid.NewGuid().ToString(),
                Name       = name,
                UpgradeIds = selected,
            });
            TxtGoalName.Text = "";
        }
    }
}
