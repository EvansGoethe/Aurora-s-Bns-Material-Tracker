using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using BnsMaterialTracker.Models;
using BnsMaterialTracker.Services;

namespace BnsMaterialTracker.ViewModels
{
    public class AppViewModel : BaseViewModel
    {
        // ── Game data ──────────────────────────────────────────
        public ObservableCollection<Dungeon>     Dungeons    { get; } = new();
        public ObservableCollection<Material>    Materials   { get; } = new();
        public ObservableCollection<UpgradeStep> AllUpgrades { get; } = new();

        // ── Save data ──────────────────────────────────────────
        private SaveData _save = new();
        public SaveData Save => _save;

        public DailyProgress  Daily    => _save.Daily;
        public WeeklyProgress Weekly   => _save.Weekly;
        public Inventory      Inventory => _save.Inventory;
        public AppSettings    Settings  => _save.Settings;
        public ObservableCollection<Goal> Goals { get; } = new();

        // ── Language change event ──────────────────────────────
        public event Action? LanguageChanged;

        // ── Navigation ─────────────────────────────────────────
        private string _currentPage = "dashboard";
        public string CurrentPage { get => _currentPage; set { if (Set(ref _currentPage, value)) OnPropertyChanged(nameof(CurrentPage)); } }

        // ── Timers for reset reminders ─────────────────────────
        private readonly List<DispatcherTimer> _timers = new();

        public AppViewModel()
        {
            Load();
            L10n.SetLanguage(Settings.Language);
            ScheduleReminders();
        }

        // ── Load ───────────────────────────────────────────────
        public void Load()
        {
            _save = DataService.LoadSave();

            // Reset stale daily/weekly
            if (_save.Daily.Date != DateHelper.TodayStr())
                _save.Daily = new DailyProgress { Date = DateHelper.TodayStr() };
            if (_save.Weekly.WeekStart != DateHelper.WeekStartStr())
                _save.Weekly = new WeeklyProgress { WeekStart = DateHelper.WeekStartStr() };

            Goals.Clear();
            foreach (var g in _save.Goals) Goals.Add(g);

            ReloadGameData();
        }

        public void ReloadGameData()
        {
            Dungeons.Clear();
            var df = DataService.LoadGameData<DungeonsFile>("dungeons.json");
            if (df != null) foreach (var d in df.Dungeons) Dungeons.Add(d);

            Materials.Clear();
            var mf = DataService.LoadGameData<MaterialsFile>("materials.json");
            if (mf != null) foreach (var m in mf.Materials) Materials.Add(m);

            AllUpgrades.Clear();
            var uf = DataService.LoadGameData<UpgradesFile>("upgrades.json");
            if (uf != null)
            {
                foreach (var u in uf.WeaponUpgrades)        AllUpgrades.Add(u);
                foreach (var u in uf.BraceletUpgrades)      AllUpgrades.Add(u);
                foreach (var u in uf.NecklaceUpgrades)      AllUpgrades.Add(u);
                foreach (var u in uf.BeltUpgrades)          AllUpgrades.Add(u);
                foreach (var u in uf.GlovesUpgrades)        AllUpgrades.Add(u);
                foreach (var u in uf.EarringUpgrades)       AllUpgrades.Add(u);
                foreach (var u in uf.SecretTokenUpgrades)   AllUpgrades.Add(u);
                foreach (var u in uf.DivineTokenUpgrades)   AllUpgrades.Add(u);
                foreach (var u in uf.SoulUpgrades)          AllUpgrades.Add(u);
                foreach (var u in uf.SpiritUpgrades)        AllUpgrades.Add(u);
                foreach (var u in uf.GuardianStoneUpgrades) AllUpgrades.Add(u);
                foreach (var u in uf.StarUpgrades)          AllUpgrades.Add(u);
                foreach (var u in uf.InnerBraceletUpgrades) AllUpgrades.Add(u);
                foreach (var u in uf.OuterBraceletUpgrades) AllUpgrades.Add(u);
            }
        }

        private void Persist()
        {
            _save.Goals = Goals.ToList();
            DataService.WriteSave(_save);
        }

        // ── Dungeon run tracking ───────────────────────────────
        public int GetDungeonRuns(string dungeonId, string type)
        {
            var runs = type == "daily" ? Daily.DungeonRuns : Weekly.DungeonRuns;
            return runs.TryGetValue(dungeonId, out var v) ? v : 0;
        }

        public void IncrementRun(Dungeon d)
        {
            var runs = d.Type == "daily" ? Daily.DungeonRuns : Weekly.DungeonRuns;
            var current = runs.TryGetValue(d.Id, out var v) ? v : 0;
            if (current >= d.Limit) return;
            runs[d.Id] = current + 1;
            Persist();
            OnPropertyChanged(nameof(Daily));
            OnPropertyChanged(nameof(Weekly));
        }

        public void DecrementRun(Dungeon d)
        {
            var runs = d.Type == "daily" ? Daily.DungeonRuns : Weekly.DungeonRuns;
            var current = runs.TryGetValue(d.Id, out var v) ? v : 0;
            if (current <= 0) return;
            runs[d.Id] = current - 1;
            Persist();
            OnPropertyChanged(nameof(Daily));
            OnPropertyChanged(nameof(Weekly));
        }

        public void ResetDaily()
        {
            _save.Daily = new DailyProgress { Date = DateHelper.TodayStr() };
            Persist();
            OnPropertyChanged(nameof(Daily));
        }

        public void ResetWeekly()
        {
            _save.Weekly = new WeeklyProgress { WeekStart = DateHelper.WeekStartStr() };
            Persist();
            OnPropertyChanged(nameof(Weekly));
        }

        // ── Inventory ──────────────────────────────────────────
        public int GetMaterial(string id) => Inventory.Materials.TryGetValue(id, out var v) ? v : 0;
        public void SetMaterial(string id, int amount)
        {
            Inventory.Materials[id] = Math.Max(0, amount);
            Persist();
            OnPropertyChanged(nameof(Inventory));
        }

        public void SetGold(int amount)
        {
            Inventory.Gold = Math.Max(0, amount);
            Persist();
        }

        // ── Market prices ──────────────────────────────────────
        public double GetMarketPrice(string id) => Settings.MarketPrices.TryGetValue(id, out var v) ? v : 0;
        public void SetMarketPrice(string id, double price)
        {
            Settings.MarketPrices[id] = price;
            Persist();
        }

        // ── Goals ──────────────────────────────────────────────
        public void AddGoal(Goal goal) { Goals.Add(goal); Persist(); }
        public void RemoveGoal(Goal goal) { Goals.Remove(goal); Persist(); }

        // ── Settings ───────────────────────────────────────────
        public void ChangeLanguage(string lang)
        {
            Settings.Language = lang;
            L10n.SetLanguage(lang);
            Persist();
            LanguageChanged?.Invoke();
        }

        public void UpdateSettings()
        {
            Persist();
            ScheduleReminders();
        }

        // ── Reset reminder scheduling ──────────────────────────
        private void ScheduleReminders()
        {
            foreach (var t in _timers) t.Stop();
            _timers.Clear();
            if (!Settings.ResetReminders) return;

            ScheduleTimer(GetNextOccurrence(Settings.DailyResetHour, 0, 1),
                Settings.ReminderMinutesBefore,
                () => ShowNotification(L10n.T("nav.gameTitle") + " — " + L10n.T("tracker.resetDaily"),
                    L10n.T("tracker.remaining", new() { ["min"] = Settings.ReminderMinutesBefore })));

            var weeklyNext = GetNextWeeklyOccurrence(Settings.WeeklyResetDay, Settings.WeeklyResetHour);
            ScheduleTimer(weeklyNext, Settings.ReminderMinutesBefore,
                () => ShowNotification(L10n.T("nav.gameTitle") + " — " + L10n.T("tracker.resetWeekly"),
                    L10n.T("tracker.remaining", new() { ["min"] = Settings.ReminderMinutesBefore })));
        }

        private void ScheduleTimer(DateTime target, int minutesBefore, Action action)
        {
            var fireAt = target.AddMinutes(-minutesBefore);
            var delay = fireAt - DateTime.Now;
            if (delay.TotalMilliseconds <= 0) return;

            var timer = new DispatcherTimer { Interval = delay };
            timer.Tick += (s, e) => { ((DispatcherTimer)s!).Stop(); action(); ScheduleReminders(); };
            timer.Start();
            _timers.Add(timer);
        }

        private static DateTime GetNextOccurrence(int hour, int minute, int intervalDays)
        {
            var now = DateTime.Now;
            var candidate = DateTime.Today.AddHours(hour).AddMinutes(minute);
            while (candidate <= now) candidate = candidate.AddDays(intervalDays);
            return candidate;
        }

        private static DateTime GetNextWeeklyOccurrence(int dayOfWeek, int hour)
        {
            var now = DateTime.Now;
            int daysUntil = ((dayOfWeek - (int)now.DayOfWeek) + 7) % 7;
            if (daysUntil == 0) daysUntil = 7;
            return DateTime.Today.AddDays(daysUntil).AddHours(hour);
        }

        private static void ShowNotification(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ── Material calculation helpers ───────────────────────
        public Dictionary<string, int> CalcRequired(IEnumerable<string> upgradeIds)
        {
            var totals = new Dictionary<string, int>();
            foreach (var uid in upgradeIds)
            {
                var step = AllUpgrades.FirstOrDefault(u => u.Id == uid);
                if (step == null) continue;
                foreach (var req in step.Requirements)
                    totals[req.MaterialId] = (totals.TryGetValue(req.MaterialId, out var v) ? v : 0) + req.Amount;
            }
            return totals;
        }

        public int CalcTotalGold(IEnumerable<string> upgradeIds)
            => AllUpgrades.Where(u => upgradeIds.Contains(u.Id)).Sum(u => u.GoldCost);

        public string GetMatName(string id) => Materials.FirstOrDefault(m => m.Id == id)?.Name ?? id;
        public string GetMatIcon(string id) => Materials.FirstOrDefault(m => m.Id == id)?.Icon ?? "📦";

        public (int Days, DateTime Target, bool Achievable) PredictGoal(Goal goal)
        {
            var required = CalcRequired(goal.UpgradeIds);
            var playRatio = CalcDailyPlayRatio();
            int maxDays = 0;
            bool achievable = true;

            foreach (var (matId, total) in required)
            {
                int have = GetMaterial(matId);
                int shortage = total - have;
                if (shortage <= 0) continue;
                double yieldPerDay = CalcDailyYield(matId) * playRatio;
                if (yieldPerDay <= 0) { achievable = false; continue; }
                int days = (int)Math.Ceiling(shortage / yieldPerDay);
                maxDays = Math.Max(maxDays, days);
            }
            return (maxDays, DateTime.Today.AddDays(maxDays), achievable);
        }

        private double CalcDailyPlayRatio()
        {
            double fullMin = 0;
            foreach (var d in Dungeons)
                fullMin += d.Type == "daily"
                    ? (d.DailyLimit ?? 5) * d.EstimatedMinutes
                    : ((d.WeeklyLimit ?? 3) * d.EstimatedMinutes) / 7.0;
            return fullMin > 0 ? Math.Min(1.0, Settings.DailyPlayMinutes / fullMin) : 1.0;
        }

        public double CalcDailyYield(string matId)
        {
            double total = 0;
            foreach (var d in Dungeons)
            {
                var drop = d.Drops.FirstOrDefault(dr => dr.MaterialId == matId);
                if (drop == null) continue;
                double avg = drop.Chance * ((drop.Min + drop.Max) / 2.0);
                total += d.Type == "daily"
                    ? avg * (d.DailyLimit ?? 5)
                    : (avg * (d.WeeklyLimit ?? 3)) / 7.0;
            }
            return total;
        }
    }
}
