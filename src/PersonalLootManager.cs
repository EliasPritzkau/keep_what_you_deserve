using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace KeepWhatYouDeserve
{
    internal static class PersonalLootManager
    {
        private static MapEvent _activeMapEvent;
        private static readonly List<PersonalLootEntry> _personalLootItems = new List<PersonalLootEntry>();
        private static bool _playerKnockedOutInCurrentEvent;
        private static bool _vanillaLootCleared;
        private static readonly HashSet<Hero> _knockedOutHeroes = new HashSet<Hero>();

        public static void BeginTrackingForCurrentMapEvent()
        {
            var mapEvent = GetPlayerMapEvent();
            if (mapEvent == null)
            {
                return;
            }

            if (!ReferenceEquals(_activeMapEvent, mapEvent))
            {
                _activeMapEvent = mapEvent;
                _personalLootItems.Clear();
                _playerKnockedOutInCurrentEvent = false;
                _vanillaLootCleared = false;
                _knockedOutHeroes.Clear();
                Logger.Debug("Tracking new map event '" + mapEvent + "'.");
            }
        }

        public static void MarkPlayerKnockedOut()
        {
            _playerKnockedOutInCurrentEvent = true;
            Logger.Info("Player knocked out, no reward will be given (loot-on-KO disabled).");
        }

        public static void MarkHeroKnockedOut(Hero hero)
        {
            if (hero == null)
            {
                return;
            }

            _knockedOutHeroes.Add(hero);
        }

        public static int RegisterKill(Agent victim, Hero killerHero, bool killerIsPlayer)
        {
            if (victim == null || victim.IsMount)
            {
                return 0;
            }

            CharacterObject victimChar = victim.Character as CharacterObject;
            if (victimChar == null)
            {
                return 0;
            }

            var mapEvent = GetPlayerMapEvent();
            if (mapEvent == null)
            {
                return 0;
            }

            if (_activeMapEvent == null)
            {
                _activeMapEvent = mapEvent;
                _playerKnockedOutInCurrentEvent = false;
                _vanillaLootCleared = false;
            }
            else if (!ReferenceEquals(_activeMapEvent, mapEvent))
            {
                _activeMapEvent = mapEvent;
                _personalLootItems.Clear();
                _playerKnockedOutInCurrentEvent = false;
                _vanillaLootCleared = false;
            }

            var spawnEquipment = victim.SpawnEquipment;
            if (spawnEquipment == null)
            {
                return 0;
            }

            // If player was knocked out and setting disallows loot on KO, block collection.
            var settings = KeepWhatYouDeserveSettings.Instance;
            bool allowLootWhenKO = settings != null ? settings.LootWhenPlayerKO : false;
            if (!allowLootWhenKO && _playerKnockedOutInCurrentEvent)
            {
                Logger.Debug("Skipping collection because player is KO and 'Loot when player knocked out' is disabled.");
                return 0;
            }

            // If killer is a companion and they are knocked out, skip (when loot-on-KO disabled).
            if (!allowLootWhenKO && killerHero != null && killerHero != Hero.MainHero && _knockedOutHeroes.Contains(killerHero))
            {
                Logger.Debug("Skipping collection because killer companion is KO and 'Loot when player knocked out' is disabled.");
                return 0;
            }

            // Victim filters: tier/hero/regular.
            bool includeHeroes = settings == null ? true : settings.IncludeHeroes;
            bool includeElites = settings == null ? true : settings.IncludeElites;
            bool includeRegulars = settings == null ? true : settings.IncludeRegulars;
            int eliteTierThreshold = settings == null ? 4 : settings.EliteTierThreshold;

            bool isHeroVictim = victimChar.IsHero;
            bool isEliteVictim = !isHeroVictim && victimChar.Tier >= eliteTierThreshold;
            bool isRegularVictim = !isHeroVictim && !isEliteVictim;

            if (isHeroVictim && !includeHeroes)
            {
                return 0;
            }
            if (isEliteVictim && !includeElites)
            {
                return 0;
            }
            if (isRegularVictim && !includeRegulars)
            {
                return 0;
            }

            int added = 0;
            var slotCount = (int)EquipmentIndex.NumEquipmentSetSlots;
            for (var i = 0; i < slotCount; i++)
            {
                var index = (EquipmentIndex)i;
                var element = spawnEquipment[index];
                if (element.Item == null || element.IsEmpty)
                {
                    continue;
                }

                if (!ShouldKeepItem(element, index, settings))
                {
                    continue;
                }

                _personalLootItems.Add(new PersonalLootEntry(element, killerHero, killerIsPlayer));
                added++;
            }

            Logger.Debug("Kill registered for map event '" + (mapEvent != null ? mapEvent.ToString() : "null") +
                         "'; added " + added + " items; running total " + _personalLootItems.Count + ".");
            return added;
        }

        public static List<EquipmentElement> ConsumePersonalLootForMapEvent(MapEvent mapEvent)
        {
            if (mapEvent == null)
            {
                Logger.Debug("ConsumePersonalLootForMapEvent: mapEvent null, nothing to deliver.");
                return new List<EquipmentElement>();
            }

            if (!ReferenceEquals(mapEvent, _activeMapEvent))
            {
                Logger.Debug("ConsumePersonalLootForMapEvent: mapEvent mismatch; active is "
                             + (_activeMapEvent != null ? _activeMapEvent.ToString() : "null"));
                return new List<EquipmentElement>();
            }

            if (_personalLootItems.Count == 0)
            {
                Logger.Debug("ConsumePersonalLootForMapEvent: no personal items collected.");
                return new List<EquipmentElement>();
            }

            var settings = KeepWhatYouDeserveSettings.Instance;
            bool allowLootWhenKO = settings != null ? settings.LootWhenPlayerKO : false;
            if (!allowLootWhenKO && _playerKnockedOutInCurrentEvent)
            {
                Logger.Debug("ConsumePersonalLootForMapEvent: player was KO and loot-on-KO disabled; discarding collected items.");
                _personalLootItems.Clear();
                _activeMapEvent = null;
                _playerKnockedOutInCurrentEvent = false;
                return new List<EquipmentElement>();
            }

            float dropChance = 100f;
            if (settings != null)
            {
                dropChance = settings.PersonalLootDropChance;
            }
            if (dropChance < 0f) dropChance = 0f;
            if (dropChance > 100f) dropChance = 100f;

            var result = new List<EquipmentElement>(_personalLootItems.Count);
            if (dropChance >= 100f)
            {
                foreach (var entry in _personalLootItems)
                {
                    if (!allowLootWhenKO && entry.KillerHero != null && entry.KillerHero != Hero.MainHero && _knockedOutHeroes.Contains(entry.KillerHero))
                    {
                        continue;
                    }

                    result.Add(entry.Element);
                }
            }
            else if (dropChance <= 0f)
            {
                Logger.Debug("ConsumePersonalLootForMapEvent: drop chance is 0%; skipping all personal items.");
            }
            else
            {
                foreach (var entry in _personalLootItems)
                {
                    if (!allowLootWhenKO && entry.KillerHero != null && entry.KillerHero != Hero.MainHero && _knockedOutHeroes.Contains(entry.KillerHero))
                    {
                        continue;
                    }

                    if (MBRandom.RandomFloat * 100f < dropChance)
                    {
                        result.Add(entry.Element);
                    }
                }
                Logger.Debug("ConsumePersonalLootForMapEvent: applied drop chance " + dropChance + "%; kept " + result.Count + " of " + _personalLootItems.Count + " items.");
            }

            Logger.Info("Delivering " + result.Count + " personal loot items to roster for map event '" + mapEvent + "'.");

            _personalLootItems.Clear();
            _activeMapEvent = null;
            _playerKnockedOutInCurrentEvent = false;
            _vanillaLootCleared = false;

            return result;
        }

        private static MapEvent GetPlayerMapEvent()
        {
            var mainParty = MobileParty.MainParty;
            if (mainParty == null)
            {
                return null;
            }

            return mainParty.MapEvent;
        }

        public static void ClearVanillaLootIfNeeded(ItemRoster roster)
        {
            if (roster == null)
            {
                return;
            }

            var settings = KeepWhatYouDeserveSettings.Instance;
            bool removeVanilla = settings != null ? settings.RemoveVanillaItemLoot : false;
            if (!removeVanilla)
            {
                return;
            }

            if (_vanillaLootCleared)
            {
                return;
            }

            roster.Clear();
            _vanillaLootCleared = true;
            Logger.Info("Vanilla item loot cleared due to setting.");
        }

        private static bool ShouldKeepItem(EquipmentElement element, EquipmentIndex index, KeepWhatYouDeserveSettings settings)
        {
            if (settings == null)
            {
                return true;
            }

            ItemObject item = element.Item;
            if (item == null)
            {
                return false;
            }

            int value = item.Value;
            if (value < settings.MinItemValue)
            {
                return false;
            }
            if (settings.MaxItemValue > 0 && value > settings.MaxItemValue)
            {
                return false;
            }

            if (settings.ExcludeCivilian && item.IsCivilian)
            {
                return false;
            }

            // Slot-based filters.
            ItemObject.ItemTypeEnum type = item.ItemType;
            bool isShield = type == ItemObject.ItemTypeEnum.Shield;
            bool isHorse = type == ItemObject.ItemTypeEnum.Horse;
            bool isHarness = type == ItemObject.ItemTypeEnum.HorseHarness;
            bool isAmmo = type == ItemObject.ItemTypeEnum.Arrows || type == ItemObject.ItemTypeEnum.Bolts;
            bool isWeapon = item.PrimaryWeapon != null && !isShield && !isAmmo && !isHorse && !isHarness;

            bool isHead = type == ItemObject.ItemTypeEnum.HeadArmor;
            bool isBody = type == ItemObject.ItemTypeEnum.BodyArmor;
            bool isHand = type == ItemObject.ItemTypeEnum.HandArmor;
            bool isLeg = type == ItemObject.ItemTypeEnum.LegArmor;
            bool isCape = type == ItemObject.ItemTypeEnum.Cape;

            if (isWeapon && !settings.IncludeWeapons)
            {
                return false;
            }
            if (isShield && !settings.IncludeShields)
            {
                return false;
            }
            if (isAmmo && !settings.IncludeAmmo)
            {
                return false;
            }
            if (isHead && !settings.IncludeHeadArmor)
            {
                return false;
            }
            if (isBody && !settings.IncludeBodyArmor)
            {
                return false;
            }
            if (isHand && !settings.IncludeHandArmor)
            {
                return false;
            }
            if (isLeg && !settings.IncludeLegArmor)
            {
                return false;
            }
            if (isCape && !settings.IncludeCapes)
            {
                return false;
            }
            if (isHorse && !settings.IncludeMounts)
            {
                return false;
            }
            if (isHarness && !settings.IncludeHarnesses)
            {
                return false;
            }

            if (settings.HighTierOnly && (int)item.Tier < settings.HighTierMinTier)
            {
                return false;
            }

            return true;
        }

        private struct PersonalLootEntry
        {
            public EquipmentElement Element;
            public Hero KillerHero;
            public bool KillerIsPlayer;

            public PersonalLootEntry(EquipmentElement element, Hero killerHero, bool killerIsPlayer)
            {
                Element = element;
                KillerHero = killerHero;
                KillerIsPlayer = killerIsPlayer;
            }
        }
    }
}
