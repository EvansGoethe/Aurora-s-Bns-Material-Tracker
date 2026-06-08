using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BnsMaterialTracker.Models;

namespace BnsMaterialTracker.Services
{
    public class DataService
    {
        private static readonly JsonSerializerOptions Opts = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        };

        // ── Paths ──────────────────────────────────────────────
        public static string AppDataDir { get; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BnsMaterialTracker");

        public static string GameDataDir { get; } =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        private static string SaveFile => Path.Combine(AppDataDir, "save.json");

        static DataService()
        {
            Directory.CreateDirectory(AppDataDir);
            Directory.CreateDirectory(GameDataDir);
        }

        // ── Save / Load ────────────────────────────────────────
        public static SaveData LoadSave()
        {
            try
            {
                if (File.Exists(SaveFile))
                    return JsonSerializer.Deserialize<SaveData>(File.ReadAllText(SaveFile), Opts) ?? new SaveData();
            }
            catch { }
            return new SaveData();
        }

        public static void WriteSave(SaveData data)
        {
            File.WriteAllText(SaveFile, JsonSerializer.Serialize(data, Opts));
        }

        // ── Game data JSON files ───────────────────────────────
        public static T? LoadGameData<T>(string filename) where T : class
        {
            var path = Path.Combine(GameDataDir, filename);
            try
            {
                if (File.Exists(path))
                    return JsonSerializer.Deserialize<T>(File.ReadAllText(path), Opts);
            }
            catch { }
            return null;
        }

        public static void WriteGameData<T>(string filename, T data)
        {
            var path = Path.Combine(GameDataDir, filename);
            File.WriteAllText(path, JsonSerializer.Serialize(data, Opts));
        }

        public static void OpenDataFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", GameDataDir);
        }
    }
}
