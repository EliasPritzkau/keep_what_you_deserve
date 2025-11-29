using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace KeepWhatYouDeserve
{
    internal class KeepWhatYouDeserveSettings : AttributeGlobalSettings<KeepWhatYouDeserveSettings>
    {
        public override string Id
        {
            get { return "KeepWhatYouDeserveSettings_v1"; }
        }

        public override string DisplayName
        {
            get { return "Keep What You Deserve"; }
        }

        public override string FolderName
        {
            get { return "KeepWhatYouDeserve"; }
        }

        public override string FormatType
        {
            get { return "json"; }
        }

        [SettingPropertyBool("Enable in-game logging", Order = 100, RequireRestart = false, HintText = "Show KWYD messages during play.")]
        [SettingPropertyGroup("Debug", GroupOrder = 100)]
        public bool EnableInGameLogging { get; set; }

        [SettingPropertyBool("Count friendly fire", Order = 1, RequireRestart = false, HintText = "If enabled, gear from friendlies you down/kill is added.")]
        [SettingPropertyGroup("Loot Rules")]
        public bool CountFriendlies { get; set; }

        [SettingPropertyBool("Count kills from companion", Order = 2, RequireRestart = false, HintText = "If enabled, gear from your companions' kills is added.")]
        [SettingPropertyGroup("Loot Rules")]
        public bool CountCompanions { get; set; }

        [SettingPropertyBool("Count nobles/lords killed", Order = 3, RequireRestart = false, HintText = "If enabled, gear from nobles/lords you down/kill is added.")]
        [SettingPropertyGroup("Loot Rules")]
        public bool CountNobles { get; set; }

        [SettingPropertyBool("Loot when hero knocked out", Order = 4, RequireRestart = false, HintText = "If enabled, personal loot is kept even if the killer hero (player or companion) is knocked out.")]
        [SettingPropertyGroup("Loot Rules")]
        public bool LootWhenPlayerKO { get; set; }

        [SettingPropertyBool("Remove vanilla item loot", Order = 5, RequireRestart = false, HintText = "If enabled, vanilla item loot is cleared; only personal loot remains (gold unaffected).")]
        [SettingPropertyGroup("Loot Rules")]
        public bool RemoveVanillaItemLoot { get; set; }

        [SettingPropertyFloatingInteger("Personal loot drop chance (%)", 0f, 100f, Order = 6, RequireRestart = false, HintText = "Chance per collected personal item to appear in loot. 100% = always, 0% = never.")]
        [SettingPropertyGroup("Loot Rules")]
        public float PersonalLootDropChance { get; set; }

        [SettingPropertyGroup("Victim Filters", GroupOrder = 10)]
        [SettingPropertyBool("Include regular troops", Order = 10, RequireRestart = false)]
        public bool IncludeRegulars { get; set; }

        [SettingPropertyGroup("Victim Filters")]
        [SettingPropertyBool("Include elite troops", Order = 11, RequireRestart = false, HintText = "Elites are defined by tier at or above the elite threshold.")]
        public bool IncludeElites { get; set; }

        [SettingPropertyGroup("Victim Filters")]
        [SettingPropertyBool("Include heroes/nobles", Order = 12, RequireRestart = false)]
        public bool IncludeHeroes { get; set; }

        [SettingPropertyGroup("Victim Filters")]
        [SettingPropertyInteger("Elite tier threshold", 1, 6, Order = 13, RequireRestart = false)]
        public int EliteTierThreshold { get; set; }

        [SettingPropertyGroup("Gear Filters", GroupOrder = 20)]
        [SettingPropertyBool("Include weapons", Order = 10, RequireRestart = false, HintText = "Include all weapons (excluding shields and ammo if those are disabled).")]
        public bool IncludeWeapons { get; set; }

        [SettingPropertyGroup("Gear Filters")]
        [SettingPropertyBool("Include shields", Order = 11, RequireRestart = false, HintText = "Include shields.")]
        public bool IncludeShields { get; set; }

        [SettingPropertyGroup("Gear Filters")]
        [SettingPropertyBool("Include ammo", Order = 12, RequireRestart = false, HintText = "Include arrows/bolts.")]
        public bool IncludeAmmo { get; set; }

        [SettingPropertyGroup("Gear Filters")]
        [SettingPropertyBool("Include head armor", Order = 20, RequireRestart = false)]
        public bool IncludeHeadArmor { get; set; }

        [SettingPropertyGroup("Gear Filters")]
        [SettingPropertyBool("Include body armor", Order = 21, RequireRestart = false)]
        public bool IncludeBodyArmor { get; set; }

        [SettingPropertyGroup("Gear Filters")]
        [SettingPropertyBool("Include hand armor", Order = 22, RequireRestart = false)]
        public bool IncludeHandArmor { get; set; }

        [SettingPropertyGroup("Gear Filters")]
        [SettingPropertyBool("Include leg armor", Order = 23, RequireRestart = false)]
        public bool IncludeLegArmor { get; set; }

        [SettingPropertyGroup("Gear Filters")]
        [SettingPropertyBool("Include capes", Order = 24, RequireRestart = false)]
        public bool IncludeCapes { get; set; }

        [SettingPropertyGroup("Gear Filters")]
        [SettingPropertyBool("Include mounts", Order = 30, RequireRestart = false)]
        public bool IncludeMounts { get; set; }

        [SettingPropertyGroup("Gear Filters")]
        [SettingPropertyBool("Include horse harnesses", Order = 31, RequireRestart = false)]
        public bool IncludeHarnesses { get; set; }

        [SettingPropertyGroup("Gear Filters")]
        [SettingPropertyInteger("Min item value", 0, 100000, Order = 40, RequireRestart = false, HintText = "Ignore items below this value.")]
        public int MinItemValue { get; set; }

        [SettingPropertyGroup("Gear Filters")]
        [SettingPropertyInteger("Max item value", 0, 1000000, Order = 41, RequireRestart = false, HintText = "Ignore items above this value. Set high to disable.")]
        public int MaxItemValue { get; set; }

        [SettingPropertyGroup("Gear Filters")]
        [SettingPropertyBool("Exclude civilian gear", Order = 42, RequireRestart = false)]
        public bool ExcludeCivilian { get; set; }

        [SettingPropertyGroup("Gear Filters")]
        [SettingPropertyBool("High-tier only", Order = 43, RequireRestart = false, HintText = "If enabled, only items at or above the specified tier are kept.")]
        public bool HighTierOnly { get; set; }

        [SettingPropertyGroup("Gear Filters")]
        [SettingPropertyInteger("High-tier minimum tier", 0, 6, Order = 44, RequireRestart = false)]
        public int HighTierMinTier { get; set; }

        public KeepWhatYouDeserveSettings()
        {
            EnableInGameLogging = false;
            CountFriendlies = false;
            CountCompanions = false;
            CountNobles = true;
            LootWhenPlayerKO = false;
            RemoveVanillaItemLoot = false;
            PersonalLootDropChance = 100f;

            IncludeWeapons = true;
            IncludeShields = true;
            IncludeAmmo = true;
            IncludeHeadArmor = true;
            IncludeBodyArmor = true;
            IncludeHandArmor = true;
            IncludeLegArmor = true;
            IncludeCapes = true;
            IncludeMounts = true;
            IncludeHarnesses = true;
            MinItemValue = 0;
            MaxItemValue = 1000000;
            ExcludeCivilian = false;
            HighTierOnly = false;
            HighTierMinTier = 5;

            IncludeRegulars = true;
            IncludeElites = true;
            IncludeHeroes = true;
            EliteTierThreshold = 4;
        }
    }
}
