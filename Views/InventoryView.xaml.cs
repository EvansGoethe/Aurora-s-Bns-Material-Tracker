using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using BnsMaterialTracker.Services;
using BnsMaterialTracker.ViewModels;

namespace BnsMaterialTracker.Views
{
    public partial class InventoryView : UserControl
    {
        private AppViewModel _vm = null!;
        private bool _updating;

        public class MatItem : INotifyPropertyChanged
        {
            public string Id { get; set; } = "";
            public string Icon { get; set; } = "📦";
            public string Name { get; set; } = "";
            private int _amount;
            public int Amount
            {
                get => _amount;
                set { _amount = value; PropertyChanged?.Invoke(this, new(nameof(Amount))); }
            }
            public event PropertyChangedEventHandler? PropertyChanged;
        }

        public class MatGroup
        {
            public string GroupLabel { get; set; } = "";
            public List<MatItem> Items { get; set; } = new();
        }

        // 分類的顯示順序和 L10n key 對應
        private static readonly (string Category, string L10nKey, string Icon)[] CategoryOrder =
        {
            ("weapon",       "inventory.weapon",       "⚔️"),
            ("bracelet",     "calc.bracelet",          "💠"),
            ("necklace",     "calc.necklace",          "📿"),
            ("belt",         "calc.belt",              "🔶"),
            ("gloves",       "calc.gloves",            "🥊"),
            ("earring",      "calc.earring",           "💎"),
            ("secretToken",  "inventory.secretToken",  "🎴"),
            ("divineToken",  "inventory.divineToken",  "🃏"),
            ("soul",         "inventory.soul",         "👻"),
            ("spirit",       "inventory.spirit",       "✨"),
            ("guardianStone","inventory.guardianStone","🛡️"),
            ("star",         "inventory.star",         "⭐"),
            ("innerBracelet","inventory.innerBracelet","🔵"),
            ("outerBracelet","inventory.outerBracelet","🔴"),
            ("exchange",     "inventory.exchange",     "🎫"),
        };

        public InventoryView() => InitializeComponent();

        public void Refresh()
        {
            if (DataContext is not AppViewModel vm) return;
            _vm = vm;

            TxtTitle.Text = L10n.T("inventory.title");
            TxtGoldLabel.Text = L10n.T("inventory.gold");

            _updating = true;
            TxtGold.Text = vm.Inventory.Gold.ToString();
            _updating = false;

            // Build dynamic group list
            var groups = new List<MatGroup>();
            foreach (var (cat, key, icon) in CategoryOrder)
            {
                var mats = vm.Materials
                    .Where(m => m.Category == cat)
                    .Select(m => new MatItem
                    {
                        Id     = m.Id,
                        Icon   = m.Icon,
                        Name   = m.Name,
                        Amount = vm.GetMaterial(m.Id),
                    }).ToList();
                if (mats.Count == 0) continue;
                groups.Add(new MatGroup
                {
                    GroupLabel = icon + " " + L10n.T(key),
                    Items      = mats,
                });
            }
            GroupList.ItemsSource = groups;
        }

        private void MatAmount_Changed(object sender, TextChangedEventArgs e)
        {
            if (_updating || _vm == null) return;
            if (sender is not TextBox tb) return;
            if (tb.Tag is not string id) return;
            if (!int.TryParse(tb.Text, out int amount)) return;
            _vm.SetMaterial(id, amount);
        }

        private void TxtGold_Changed(object sender, TextChangedEventArgs e)
        {
            if (_updating || _vm == null) return;
            if (int.TryParse(TxtGold.Text, out int gold))
                _vm.SetGold(gold);
        }
    }
}
