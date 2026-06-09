using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BnsMaterialTracker.Models
{
    public class MaterialDrop
    {
        [JsonPropertyName("materialId")] public string MaterialId { get; set; } = "";
        [JsonPropertyName("chance")]     public double Chance { get; set; } = 1.0;
        [JsonPropertyName("min")]        public int Min { get; set; } = 1;
        [JsonPropertyName("max")]        public int Max { get; set; } = 1;
    }

    public class Dungeon
    {
        [JsonPropertyName("id")]             public string Id { get; set; } = "";
        [JsonPropertyName("name")]           public string Name { get; set; } = "";
        [JsonPropertyName("shortName")]      public string ShortName { get; set; } = "";
        [JsonPropertyName("type")]           public string Type { get; set; } = "daily";   // daily | weekly
        [JsonPropertyName("mode")]           public string Mode { get; set; } = "hero";   // hero | demon
        [JsonPropertyName("difficulty")]     public string Difficulty { get; set; } = "normal"; // hero: easy/normal/skilled | demon: 1-7
        [JsonPropertyName("dailyLimit")]     public int? DailyLimit { get; set; }
        [JsonPropertyName("weeklyLimit")]    public int? WeeklyLimit { get; set; }
        [JsonPropertyName("estimatedMinutes")] public int EstimatedMinutes { get; set; } = 20;
        [JsonPropertyName("drops")]          public List<MaterialDrop> Drops { get; set; } = new();

        [JsonIgnore] public int Limit => Type == "daily" ? (DailyLimit ?? 5) : (WeeklyLimit ?? 3);
    }

    public class Material
    {
        [JsonPropertyName("id")]       public string Id { get; set; } = "";
        [JsonPropertyName("name")]     public string Name { get; set; } = "";
        [JsonPropertyName("category")] public string Category { get; set; } = "weapon";
        [JsonPropertyName("icon")]     public string Icon { get; set; } = "📦";
    }

    public class UpgradeRequirement
    {
        [JsonPropertyName("materialId")] public string MaterialId { get; set; } = "";
        [JsonPropertyName("amount")]     public int Amount { get; set; } = 1;
    }

    public class UpgradeStep
    {
        [JsonPropertyName("id")]           public string Id { get; set; } = "";
        [JsonPropertyName("name")]         public string Name { get; set; } = "";
        [JsonPropertyName("category")]     public string Category { get; set; } = "weapon";
        [JsonPropertyName("fromStage")]    public int FromStage { get; set; } = 1;
        [JsonPropertyName("toStage")]      public int ToStage { get; set; } = 2;
        [JsonPropertyName("requirements")] public List<UpgradeRequirement> Requirements { get; set; } = new();
        [JsonPropertyName("goldCost")]     public int GoldCost { get; set; } = 0;
    }

    public class DungeonsFile   { [JsonPropertyName("dungeons")]          public List<Dungeon> Dungeons { get; set; } = new(); }
    public class MaterialsFile  { [JsonPropertyName("materials")]         public List<Material> Materials { get; set; } = new(); }
    public class UpgradesFile
    {
        // 武器
        [JsonPropertyName("weaponUpgrades")]        public List<UpgradeStep> WeaponUpgrades        { get; set; } = new();
        // 首飾細項
        [JsonPropertyName("braceletUpgrades")]      public List<UpgradeStep> BraceletUpgrades      { get; set; } = new();
        [JsonPropertyName("necklaceUpgrades")]      public List<UpgradeStep> NecklaceUpgrades      { get; set; } = new();
        [JsonPropertyName("beltUpgrades")]          public List<UpgradeStep> BeltUpgrades          { get; set; } = new();
        [JsonPropertyName("glovesUpgrades")]        public List<UpgradeStep> GlovesUpgrades        { get; set; } = new();
        [JsonPropertyName("earringUpgrades")]       public List<UpgradeStep> EarringUpgrades       { get; set; } = new();
        // 其他大項
        [JsonPropertyName("secretTokenUpgrades")]   public List<UpgradeStep> SecretTokenUpgrades   { get; set; } = new();
        [JsonPropertyName("divineTokenUpgrades")]   public List<UpgradeStep> DivineTokenUpgrades   { get; set; } = new();
        [JsonPropertyName("soulUpgrades")]          public List<UpgradeStep> SoulUpgrades          { get; set; } = new();
        [JsonPropertyName("spiritUpgrades")]        public List<UpgradeStep> SpiritUpgrades        { get; set; } = new();
        [JsonPropertyName("guardianStoneUpgrades")] public List<UpgradeStep> GuardianStoneUpgrades { get; set; } = new();
        [JsonPropertyName("starUpgrades")]          public List<UpgradeStep> StarUpgrades          { get; set; } = new();
        [JsonPropertyName("innerBraceletUpgrades")]     public List<UpgradeStep> InnerBraceletUpgrades     { get; set; } = new();
        [JsonPropertyName("outerBraceletUpgrades")]     public List<UpgradeStep> OuterBraceletUpgrades     { get; set; } = new();
        // 隕石升級
        [JsonPropertyName("waterMeteoriteUpgrades")]     public List<UpgradeStep> WaterMeteoriteUpgrades     { get; set; } = new();
        [JsonPropertyName("woodMeteoriteUpgrades")]      public List<UpgradeStep> WoodMeteoriteUpgrades      { get; set; } = new();
        [JsonPropertyName("fireMeteoriteUpgrades")]      public List<UpgradeStep> FireMeteoriteUpgrades      { get; set; } = new();
        [JsonPropertyName("earthMeteoriteUpgrades")]     public List<UpgradeStep> EarthMeteoriteUpgrades     { get; set; } = new();
        [JsonPropertyName("lightningMeteoriteUpgrades")] public List<UpgradeStep> LightningMeteoriteUpgrades { get; set; } = new();
    }
}
