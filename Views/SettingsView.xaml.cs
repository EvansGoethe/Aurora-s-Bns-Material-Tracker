using System.Windows;
using System.Windows.Controls;
using BnsMaterialTracker.Services;
using BnsMaterialTracker.ViewModels;

namespace BnsMaterialTracker.Views
{
    public partial class SettingsView : UserControl
    {
        private AppViewModel _vm = null!;
        private bool _loading;

        public SettingsView() => InitializeComponent();

        public void Refresh()
        {
            if (DataContext is not AppViewModel vm) return;
            _vm = vm;

            // Labels
            TxtTitle.Text          = L10n.T("settings.title");
            TxtLangTitle.Text      = L10n.T("settings.language");
            TxtPlayTitle.Text      = L10n.T("settings.playTime");
            TxtPlayDesc.Text       = L10n.T("settings.playTimeDesc");
            TxtReminderTitle.Text  = L10n.T("settings.resetReminders");
            TxtReminderDesc.Text   = L10n.T("settings.reminderDesc");
            TxtBeforeLabel.Text    = L10n.T("settings.reminderBefore");
            TxtDailyResetTitle.Text= L10n.T("settings.dailyReset");
            TxtDailyHourLabel.Text = L10n.T("settings.dailyResetTime");
            TxtWeeklyResetTitle.Text=L10n.T("settings.weeklyReset");
            TxtWeeklyDayLabel.Text = L10n.T("settings.weeklyDay");
            TxtWeeklyHourLabel.Text= L10n.T("settings.weeklyTime");
            TxtTestNotif.Text      = "🔔 " + L10n.T("settings.testNotif");
            TxtHint1.Text          = "ℹ️ " + L10n.T("settings.hint1");
            TxtHint2.Text          = "ℹ️ " + L10n.T("settings.hint2");
            TxtHint3.Text          = "ℹ️ " + L10n.T("settings.hint3");

            // Values
            _loading = true;
            var s = vm.Settings;

            // Language buttons highlight
            SetLangHighlight(s.Language);

            // Play time slider
            SliderPlay.Value  = s.DailyPlayMinutes;
            TxtPlayValue.Text = $"{s.DailyPlayMinutes} " + L10n.T("common.mins");

            // Reminders
            ChkReminders.IsChecked = s.ResetReminders;
            PanelReminderDetail.IsEnabled = s.ResetReminders;
            TxtMinsBefore.Text = s.ReminderMinutesBefore.ToString();
            TxtDailyHour.Text  = s.DailyResetHour.ToString();
            TxtWeeklyHour.Text = s.WeeklyResetHour.ToString();

            // Weekly day combo
            CmbWeeklyDay.Items.Clear();
            var days = L10n.Days();
            for (int i = 0; i < days.Length; i++)
                CmbWeeklyDay.Items.Add($"週{days[i]}");
            CmbWeeklyDay.SelectedIndex = s.WeeklyResetDay;

            _loading = false;
        }

        private void SetLangHighlight(string lang)
        {
            BtnZhTW.Style = lang == "zh-TW"
                ? (Style)FindResource("BtnPrimary") : (Style)FindResource("BtnGhost");
            BtnZhCN.Style = lang == "zh-CN"
                ? (Style)FindResource("BtnPrimary") : (Style)FindResource("BtnGhost");
            BtnEn.Style   = lang == "en"
                ? (Style)FindResource("BtnPrimary") : (Style)FindResource("BtnGhost");
        }

        private void BtnLang_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string lang && _vm != null)
                _vm.ChangeLanguage(lang);
        }

        private void SliderPlay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loading || _vm == null) return;
            _vm.Settings.DailyPlayMinutes = (int)SliderPlay.Value;
            TxtPlayValue.Text = $"{_vm.Settings.DailyPlayMinutes} " + L10n.T("common.mins");
            _vm.UpdateSettings();
        }

        private void ChkReminders_Changed(object sender, RoutedEventArgs e)
        {
            if (_loading || _vm == null) return;
            _vm.Settings.ResetReminders = ChkReminders.IsChecked == true;
            PanelReminderDetail.IsEnabled = _vm.Settings.ResetReminders;
            _vm.UpdateSettings();
        }

        private void Settings_Changed(object sender, TextChangedEventArgs e)
        {
            if (_loading || _vm == null) return;
            if (int.TryParse(TxtMinsBefore.Text, out int mins))
                _vm.Settings.ReminderMinutesBefore = mins;
            if (int.TryParse(TxtDailyHour.Text, out int dh))
                _vm.Settings.DailyResetHour = System.Math.Clamp(dh, 0, 23);
            if (int.TryParse(TxtWeeklyHour.Text, out int wh))
                _vm.Settings.WeeklyResetHour = System.Math.Clamp(wh, 0, 23);
            _vm.UpdateSettings();
        }

        private void CmbWeeklyDay_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_loading || _vm == null) return;
            _vm.Settings.WeeklyResetDay = CmbWeeklyDay.SelectedIndex;
            _vm.UpdateSettings();
        }

        private void BtnTestNotif_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                L10n.T("tracker.remaining", new() { ["min"] = _vm?.Settings.ReminderMinutesBefore ?? 15 }),
                L10n.T("nav.gameTitle") + " — " + L10n.T("tracker.resetDaily"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
