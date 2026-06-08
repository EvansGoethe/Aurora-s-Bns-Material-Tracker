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
    public partial class MarketView : UserControl
    {
        private AppViewModel _vm = null!;
        private bool _updating;

        public class PriceItem : INotifyPropertyChanged
        {
            public string Id { get; set; } = "";
            public string Icon { get; set; } = "📦";
            public string Name { get; set; } = "";
            private double _price;
            public double Price
            {
                get => _price;
                set { _price = value; PropertyChanged?.Invoke(this, new(nameof(Price))); }
            }
            public event PropertyChangedEventHandler? PropertyChanged;
        }

        public MarketView() => InitializeComponent();

        public void Refresh()
        {
            if (DataContext is not AppViewModel vm) return;
            _vm = vm;

            TxtTitle.Text      = L10n.T("market.title");
            TxtSubtitle.Text   = L10n.T("market.subtitle");
            TxtPriceLabel.Text = "💲 " + L10n.T("market.priceLabel");
            TxtEffLabel.Text   = "⚔️ " + L10n.T("nav.tracker") + " " + L10n.T("market.title");

            _updating = true;
            // Show all materials that appear in dungeon drops
            var droppedMats = vm.Dungeons
                .SelectMany(d => d.Drops.Select(dr => dr.MaterialId))
                .Distinct()
                .ToHashSet();

            PriceList.ItemsSource = vm.Materials
                .Where(m => droppedMats.Contains(m.Id))
                .Select(m => new PriceItem
                {
                    Id    = m.Id,
                    Icon  = m.Icon,
                    Name  = m.Name,
                    Price = vm.GetMarketPrice(m.Id),
                }).ToList();
            _updating = false;

            UpdateEfficiency();
        }

        private void UpdateEfficiency()
        {
            if (_vm == null) return;

            var gold = new SolidColorBrush(Color.FromRgb(234, 179, 8));
            var blue = new SolidColorBrush(Color.FromRgb(37, 99, 235));
            var purp = new SolidColorBrush(Color.FromRgb(168, 85, 247));
            var grey = new SolidColorBrush(Color.FromRgb(107, 114, 128));

            var items = _vm.Dungeons.Select(d =>
            {
                double goldPerRun = d.Drops.Sum(dr =>
                {
                    double avg   = dr.Chance * ((dr.Min + dr.Max) / 2.0);
                    double price = _vm.GetMarketPrice(dr.MaterialId);
                    return avg * price;
                });
                double goldPerMin = d.EstimatedMinutes > 0 ? goldPerRun / d.EstimatedMinutes : 0;
                double avgPerRun  = d.Drops.Sum(dr => dr.Chance * ((dr.Min + dr.Max) / 2.0));
                double timePerUnit= avgPerRun > 0 ? d.EstimatedMinutes / avgPerRun : 0;
                string type = d.Type == "daily"
                    ? L10n.T("common.daily") : L10n.T("common.weekly");
                return new { Dungeon = d, GoldPerMin = goldPerMin, AvgPerRun = avgPerRun, TimePerUnit = timePerUnit, Type = type };
            })
            .Where(x => x.GoldPerMin > 0 || x.AvgPerRun > 0)
            .OrderByDescending(x => x.GoldPerMin)
            .ToList();

            TxtNoData.Visibility = items.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;

            DungeonEffList.ItemsSource = items.Select((x, i) =>
            {
                Brush rankBg = i == 0 ? gold : i == 1 ? grey : i == 2 ? new SolidColorBrush(Color.FromRgb(180, 120, 60)) : blue;
                string best = i == 0 ? $"★ {L10n.T("market.best")} " : "";
                return new
                {
                    Rank       = $"{i + 1}",
                    RankBg     = rankBg,
                    Name       = best + x.Dungeon.Name,
                    TypeLabel  = x.Type,
                    GoldPerMin = $"{x.GoldPerMin:F2}",
                    TimePerUnit= $"{x.TimePerUnit:F1}",
                    AvgPerRun  = $"{x.AvgPerRun:F1}",
                };
            }).ToList();
        }

        private void Price_Changed(object sender, TextChangedEventArgs e)
        {
            if (_updating || _vm == null) return;
            if (sender is not TextBox tb) return;
            if (tb.Tag is not string id) return;
            if (!double.TryParse(tb.Text, out double price)) return;
            _vm.SetMarketPrice(id, price);
            UpdateEfficiency();
        }
    }
}
