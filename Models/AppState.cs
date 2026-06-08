using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BnsMaterialTracker.Models
{
    public class DailyProgress
    {
        [JsonPropertyName("date")]        public string Date { get; set; } = "";
        [JsonPropertyName("dungeonRuns")] public Dictionary<string, int> DungeonRuns { get; set; } = new();
    }

    public class WeeklyProgress
    {
        [JsonPropertyName("weekStart")]   public string WeekStart { get; set; } = "";
        [JsonPropertyName("dungeonRuns")] public Dictionary<string, int> DungeonRuns { get; set; } = new();
    }

    public class Inventory
    {
        [JsonPropertyName("materials")] public Dictionary<string, int> Materials { get; set; } = new();
        [JsonPropertyName("gold")]      public int Gold { get; set; } = 0;
    }

    public class Goal
    {
        [JsonPropertyName("id")]         public string Id { get; set; } = "";
        [JsonPropertyName("name")]       public string Name { get; set; } = "";
        [JsonPropertyName("upgradeIds")] public List<string> UpgradeIds { get; set; } = new();
        [JsonPropertyName("createdAt")]  public string CreatedAt { get; set; } = "";
    }

    public class AppSettings
    {
        [JsonPropertyName("language")]              public string Language { get; set; } = "zh-TW";
        [JsonPropertyName("dailyPlayMinutes")]       public int DailyPlayMinutes { get; set; } = 60;
        [JsonPropertyName("resetReminders")]        public bool ResetReminders { get; set; } = false;
        [JsonPropertyName("dailyResetHour")]        public int DailyResetHour { get; set; } = 6;
        [JsonPropertyName("weeklyResetDay")]        public int WeeklyResetDay { get; set; } = 3;
        [JsonPropertyName("weeklyResetHour")]       public int WeeklyResetHour { get; set; } = 6;
        [JsonPropertyName("reminderMinutesBefore")] public int ReminderMinutesBefore { get; set; } = 10;
        [JsonPropertyName("marketPrices")]          public Dictionary<string, double> MarketPrices { get; set; } = new();
    }

    public class SaveData
    {
        [JsonPropertyName("daily")]     public DailyProgress Daily { get; set; } = new();
        [JsonPropertyName("weekly")]    public WeeklyProgress Weekly { get; set; } = new();
        [JsonPropertyName("inventory")] public Inventory Inventory { get; set; } = new();
        [JsonPropertyName("goals")]     public List<Goal> Goals { get; set; } = new();
        [JsonPropertyName("settings")]  public AppSettings Settings { get; set; } = new();
    }

    public static class DateHelper
    {
        public static string TodayStr() => DateTime.Today.ToString("yyyy-MM-dd");
        public static string WeekStartStr()
        {
            var today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            return today.AddDays(-diff).ToString("yyyy-MM-dd");
        }
    }
}
