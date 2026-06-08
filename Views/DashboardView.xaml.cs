using System;
using System.Linq;
using System.Windows.Controls;
using BnsMaterialTracker.Services;
using BnsMaterialTracker.ViewModels;

namespace BnsMaterialTracker.Views
{
    public partial class DashboardView : UserControl
    {
        private AppViewModel _vm = null!;

        public DashboardView() => InitializeComponent();

        public void Refresh()
        {
            if (DataContext is not AppViewModel vm) return;
            _vm = vm;

            // L10n labels
            TxtTitle.Text       = L10n.T("dashboard.title");
            StatTimeLabel.Text  = L10n.T("dashboard.estTime");
            StatDailyLabel.Text = L10n.T("dashboard.dailyProg");
            StatWeeklyLabel.Text= L10n.T("dashboard.weeklyProg");
            StatGoldLabel.Text  = L10n.T("common.gold");
            TxtDailyTitle.Text  = L10n.T("dashboard.dailyDungeons");
            TxtWeeklyTitle.Text = L10n.T("dashboard.weeklyDungeons");
            TxtGoalsTitle.Text  = L10n.T("dashboard.goals");

            // Stats
            var daily   = vm.Dungeons.Where(d => d.Type == "daily").ToList();
            var weekly  = vm.Dungeons.Where(d => d.Type != "daily").ToList();
            int dDone   = daily.Sum(d  => vm.GetDungeonRuns(d.Id, "daily"));
            int dTotal  = daily.Sum(d  => d.DailyLimit  ?? 5);
            int wDone   = weekly.Sum(d => vm.GetDungeonRuns(d.Id, "weekly"));
            int wTotal  = weekly.Sum(d => d.WeeklyLimit ?? 3);

            StatDaily.Text  = $"{dDone} / {dTotal}";
            StatWeekly.Text = $"{wDone} / {wTotal}";
            StatGold.Text   = vm.Inventory.Gold.ToString();

            // Estimate remaining time
            double remMin = 0;
            foreach (var d in daily)
            {
                int limit = d.DailyLimit ?? 5;
                int done  = vm.GetDungeonRuns(d.Id, "daily");
                remMin += (limit - done) * d.EstimatedMinutes;
            }
            StatTime.Text = $"{(int)remMin} " + L10n.T("common.min");

            // Daily dungeon list
            var dItems = daily.Select(d =>
            {
                int limit = d.DailyLimit ?? 5;
                int done  = vm.GetDungeonRuns(d.Id, "daily");
                return new { Name = d.Name, Pct = limit > 0 ? (double)done / limit * 100 : 0,
                             Label = $"{done}/{limit}" };
            }).ToList();
            DailyList.ItemsSource = dItems;
            TxtDailyEmpty.Visibility = dItems.Count == 0
                ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            // Weekly dungeon list
            var wItems = weekly.Select(d =>
            {
                int limit = d.WeeklyLimit ?? 3;
                int done  = vm.GetDungeonRuns(d.Id, "weekly");
                return new { Name = d.Name, Pct = limit > 0 ? (double)done / limit * 100 : 0,
                             Label = $"{done}/{limit}" };
            }).ToList();
            WeeklyList.ItemsSource = wItems;
            TxtWeeklyEmpty.Visibility = wItems.Count == 0
                ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            // Goals
            var gItems = vm.Goals.Select(g =>
            {
                var (days, target, ok) = vm.PredictGoal(g);
                string eta = !ok ? L10n.T("predictor.noSource")
                           : days == 0 ? L10n.T("predictor.done")
                           : $"{days} " + L10n.T("common.days");
                return new { Name = g.Name, Steps = $"{g.UpgradeIds.Count} " + L10n.T("dashboard.steps"), Eta = eta };
            }).ToList();
            GoalsList.ItemsSource = gItems;
            TxtGoalsEmpty.Visibility = gItems.Count == 0
                ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }
    }
}
