using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BnsMaterialTracker.Services;
using BnsMaterialTracker.ViewModels;

namespace BnsMaterialTracker.Views
{
    public partial class PredictorView : UserControl
    {
        private AppViewModel _vm = null!;

        public PredictorView() => InitializeComponent();

        public void Refresh()
        {
            if (DataContext is not AppViewModel vm) return;
            _vm = vm;

            TxtTitle.Text      = L10n.T("predictor.title");
            TxtNoGoals.Text    = L10n.T("predictor.noGoals");
            TxtNoGoalsHint.Text = L10n.T("predictor.noGoalsHint");

            // Play-time info
            double fullMin = 0;
            foreach (var d in vm.Dungeons)
                fullMin += d.Type == "daily"
                    ? (d.DailyLimit ?? 5) * d.EstimatedMinutes
                    : ((d.WeeklyLimit ?? 3) * d.EstimatedMinutes) / 7.0;
            double pct = fullMin > 0
                ? Math.Min(100.0, vm.Settings.DailyPlayMinutes / fullMin * 100) : 100;
            TxtPlayTimeInfo.Text = L10n.T("predictor.fullRunTime",
                new() { ["min"] = (int)fullMin, ["pct"] = (int)pct });

            bool hasGoals = vm.Goals.Count > 0;
            CardEmpty.Visibility  = hasGoals ? Visibility.Collapsed : Visibility.Visible;
            GoalsList.Visibility  = hasGoals ? Visibility.Visible   : Visibility.Collapsed;

            if (!hasGoals) return;

            var green  = new SolidColorBrush(Color.FromRgb(34, 197, 94));
            var yellow = new SolidColorBrush(Color.FromRgb(234, 179, 8));
            var red    = new SolidColorBrush(Color.FromRgb(239, 68, 68));
            var blue   = new SolidColorBrush(Color.FromRgb(37, 99, 235));

            GoalsList.ItemsSource = vm.Goals.Select(g =>
            {
                var (days, target, ok) = vm.PredictGoal(g);

                string daysLbl  = !ok ? L10n.T("predictor.noSourceLabel")
                                : days == 0 ? L10n.T("predictor.done")
                                : $"{days} {L10n.T("common.days")}";
                string dateLbl  = days == 0 ? "" : target.ToString("MM/dd");
                string daysDesc = L10n.T("predictor.daysLeft", new() { ["n"] = days });
                string dateDesc = days > 0 ? L10n.T("predictor.estDate",
                    new() { ["date"] = target.ToString("yyyy/MM/dd") }) : "";

                Brush daysColor = !ok ? red : days == 0 ? green : days < 7 ? green : days < 30 ? yellow : blue;

                // Find bottleneck material
                var required = vm.CalcRequired(g.UpgradeIds);
                string bottleneck = "";
                int maxDays = 0;
                foreach (var (matId, total) in required)
                {
                    int have = vm.GetMaterial(matId);
                    int shortage = total - have;
                    if (shortage <= 0) continue;
                    double yield = vm.CalcDailyYield(matId);
                    if (yield <= 0) continue;
                    int d2 = (int)Math.Ceiling(shortage / yield);
                    if (d2 > maxDays) { maxDays = d2; bottleneck = $"⚡ {L10n.T("predictor.bottleneck")}: {vm.GetMatIcon(matId)} {vm.GetMatName(matId)}"; }
                }

                return new
                {
                    GoalId      = g.Id,
                    Name        = g.Name,
                    StepsLabel  = $"{g.UpgradeIds.Count} {L10n.T("predictor.steps")}",
                    DaysLabel   = daysLbl,
                    DaysColor   = daysColor,
                    DaysDesc    = daysDesc,
                    DateLabel   = dateLbl,
                    DateDesc    = dateDesc,
                    Bottleneck  = bottleneck,
                };
            }).ToList();
        }

        private void BtnRemoveGoal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string id && _vm != null)
            {
                var goal = _vm.Goals.FirstOrDefault(g => g.Id == id);
                if (goal != null) _vm.RemoveGoal(goal);
                Refresh();
            }
        }
    }
}
