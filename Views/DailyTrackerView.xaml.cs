using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BnsMaterialTracker.Models;
using BnsMaterialTracker.Services;
using BnsMaterialTracker.ViewModels;

namespace BnsMaterialTracker.Views
{
    public partial class DailyTrackerView : UserControl
    {
        private AppViewModel _vm = null!;

        // Lightweight row model to avoid anonymous types
        private record DungeonRow(string Id, string Name, string Type,
            int Done, int Limit, double Pct,
            string CountLabel, string EstLabel, string SubLabel,
            Brush BarColor);

        public DailyTrackerView() => InitializeComponent();

        public void Refresh()
        {
            if (DataContext is not AppViewModel vm) return;
            _vm = vm;

            TxtTitle.Text        = L10n.T("tracker.title");
            TxtResetDaily.Text   = "↺ " + L10n.T("tracker.resetDaily");
            TxtResetWeekly.Text  = "↺ " + L10n.T("tracker.resetWeekly");
            TxtDailyHeader.Text  = L10n.T("common.daily") + " " + L10n.T("nav.tracker");
            TxtWeeklyHeader.Text = L10n.T("common.weekly") + " " + L10n.T("nav.tracker");
            string emptyHint     = L10n.T("tracker.emptyHint");
            string perRun        = L10n.T("tracker.perRun");
            string complete      = L10n.T("tracker.complete");
            string remaining     = L10n.T("common.min");

            var blues  = new SolidColorBrush(Color.FromRgb(37, 99, 235));
            var greens = new SolidColorBrush(Color.FromRgb(34, 197, 94));

            var daily = vm.Dungeons.Where(d => d.Type == "daily")
                .Select(d =>
                {
                    int limit = d.DailyLimit ?? 5;
                    int done  = vm.GetDungeonRuns(d.Id, "daily");
                    bool full = done >= limit;
                    double pct = limit > 0 ? (double)done / limit * 100 : 0;
                    string countLbl = full ? complete : $"{done} / {limit}";
                    string estLbl   = $"≈{d.EstimatedMinutes} " + L10n.T("common.min") + "/" + L10n.T("common.times");
                    string subLbl   = full ? "" : $"{(limit - done) * d.EstimatedMinutes} {remaining} {L10n.T("tracker.remaining").Replace("{{min}}", ((limit - done) * d.EstimatedMinutes).ToString())}";
                    Brush bar = full ? greens : blues;
                    return new DungeonRow(d.Id, d.Name, "daily", done, limit, pct, countLbl, estLbl, subLbl, bar);
                }).ToList();

            DailyList.ItemsSource = daily;
            TxtDailyEmpty.Text = emptyHint;
            TxtDailyEmpty.Visibility = daily.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;

            var weekly = vm.Dungeons.Where(d => d.Type != "daily")
                .Select(d =>
                {
                    int limit = d.WeeklyLimit ?? 3;
                    int done  = vm.GetDungeonRuns(d.Id, "weekly");
                    bool full = done >= limit;
                    double pct = limit > 0 ? (double)done / limit * 100 : 0;
                    string countLbl = full ? complete : $"{done} / {limit}";
                    string estLbl   = $"≈{d.EstimatedMinutes} " + L10n.T("common.min") + "/" + L10n.T("common.times");
                    string subLbl   = full ? "" : $"{(limit - done) * d.EstimatedMinutes} {remaining} {L10n.T("tracker.remaining").Replace("{{min}}", ((limit - done) * d.EstimatedMinutes).ToString())}";
                    var purp = new SolidColorBrush(Color.FromRgb(168, 85, 247));
                    Brush bar = full ? greens : purp;
                    return new DungeonRow(d.Id, d.Name, "weekly", done, limit, pct, countLbl, estLbl, subLbl, bar);
                }).ToList();

            WeeklyList.ItemsSource = weekly;
            TxtWeeklyEmpty.Text = emptyHint;
            TxtWeeklyEmpty.Visibility = weekly.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DungeonRow row)
            {
                var d = _vm.Dungeons.FirstOrDefault(x => x.Id == row.Id);
                if (d != null) _vm.IncrementRun(d);
                Refresh();
            }
        }

        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DungeonRow row)
            {
                var d = _vm.Dungeons.FirstOrDefault(x => x.Id == row.Id);
                if (d != null) _vm.DecrementRun(d);
                Refresh();
            }
        }

        private void BtnPlusW_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DungeonRow row)
            {
                var d = _vm.Dungeons.FirstOrDefault(x => x.Id == row.Id);
                if (d != null) _vm.IncrementRun(d);
                Refresh();
            }
        }

        private void BtnMinusW_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DungeonRow row)
            {
                var d = _vm.Dungeons.FirstOrDefault(x => x.Id == row.Id);
                if (d != null) _vm.DecrementRun(d);
                Refresh();
            }
        }

        private void BtnResetDaily_Click(object sender, RoutedEventArgs e)
        {
            _vm?.ResetDaily();
            Refresh();
        }

        private void BtnResetWeekly_Click(object sender, RoutedEventArgs e)
        {
            _vm?.ResetWeekly();
            Refresh();
        }
    }
}
