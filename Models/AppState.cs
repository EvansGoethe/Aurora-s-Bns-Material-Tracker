using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

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
        [JsonPropertyName("bagTemplates")]          public List<BagTemplate> BagTemplates { get; set; } = new();
        [JsonPropertyName("bagCellSize")]           public int BagCellSize { get; set; } = 64;
    }

    /// <summary>
    /// Stores one registered template for bag scanning.
    /// The template is a 40×40 Bgr32 pixel snapshot of an item icon, used for template matching.
    /// CenterX/CenterY record where in the screenshot this item was clicked (image coordinates).
    /// </summary>
    public class BagTemplate
    {
        [JsonPropertyName("materialId")]     public string MaterialId     { get; set; } = "";
        [JsonPropertyName("templateBase64")] public string TemplateBase64 { get; set; } = "";
        [JsonPropertyName("centerX")]        public int    CenterX        { get; set; }
        [JsonPropertyName("centerY")]        public int    CenterY        { get; set; }

        [JsonIgnore] private byte[]? _cache;

        /// <summary>Raw 40×40 Bgr32 bytes (6 400 bytes). Lazily decoded from TemplateBase64.</summary>
        [JsonIgnore]
        public byte[] PixelData
        {
            get
            {
                if (_cache is null && !string.IsNullOrEmpty(TemplateBase64))
                {
                    try   { _cache = Convert.FromBase64String(TemplateBase64); }
                    catch { _cache = Array.Empty<byte>(); }
                }
                return _cache ?? Array.Empty<byte>();
            }
        }

        public void StorePixels(byte[] pixels)
        {
            _cache         = pixels;
            TemplateBase64 = Convert.ToBase64String(pixels);
        }

        /// <summary>Create a displayable BitmapSource preview from the stored pixels.</summary>
        [JsonIgnore]
        public BitmapSource? Preview
        {
            get
            {
                var px = PixelData;
                if (px.Length < 40 * 40 * 4) return null;
                var wb = new WriteableBitmap(40, 40, 96, 96,
                    System.Windows.Media.PixelFormats.Bgr32, null);
                wb.WritePixels(new System.Windows.Int32Rect(0, 0, 40, 40), px, 40 * 4, 0);
                return wb;
            }
        }
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
