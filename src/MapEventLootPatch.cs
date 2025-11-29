using System;
using System.Collections.Generic;
using HarmonyLib;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace KeepWhatYouDeserve
{
    [HarmonyPatch(typeof(MapEvent))]
    internal static class MapEventLootPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("ItemRosterForPlayerLootShare")]
        private static void ItemRosterForPlayerLootSharePostfix(
            MapEvent __instance,
            PartyBase party,
            ItemRoster __result)
        {
            try
            {
                Logger.Debug("ItemRosterForPlayerLootShare postfix entered.");

                if (party == null || __result == null)
                {
                    Logger.Debug("ItemRosterForPlayerLootShare: party or result null.");
                    return;
                }

                var mobileParty = party.MobileParty;
                if (mobileParty != MobileParty.MainParty)
                {
                    Logger.Debug("ItemRosterForPlayerLootShare: not main party, skipping.");
                    return;
                }

                PersonalLootManager.ClearVanillaLootIfNeeded(__result);

                List<EquipmentElement> extraItems = PersonalLootManager.ConsumePersonalLootForMapEvent(__instance);
                if (extraItems == null || extraItems.Count == 0)
                {
                    Logger.Debug("ItemRosterForPlayerLootShare: no extra items to add.");
                    return;
                }

                Logger.Info("MapEvent loot patch adding " + extraItems.Count + " items to player loot roster.");

                foreach (var equipmentElement in extraItems)
                {
                    if (equipmentElement.Item == null || equipmentElement.IsEmpty)
                    {
                        continue;
                    }

                    __result.AddToCounts(equipmentElement, 1);
                }

                Logger.Debug("Player loot roster now has " + __result.Count + " entries after KWYD injection.");
            }
            catch (Exception)
            {
                // Do not let errors here break the loot screen.
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("GetItemRosterReceivingLootShare")]
        private static void GetItemRosterReceivingLootSharePostfix(
            MapEvent __instance,
            PartyBase party,
            ItemRoster __result)
        {
            try
            {
                Logger.Debug("GetItemRosterReceivingLootShare postfix entered.");

                if (party == null || __result == null)
                {
                    Logger.Debug("GetItemRosterReceivingLootShare: party or result null.");
                    return;
                }

                var mobileParty = party.MobileParty;
                if (mobileParty != MobileParty.MainParty)
                {
                    Logger.Debug("GetItemRosterReceivingLootShare: not main party, skipping.");
                    return;
                }

                PersonalLootManager.ClearVanillaLootIfNeeded(__result);

                List<EquipmentElement> extraItems = PersonalLootManager.ConsumePersonalLootForMapEvent(__instance);
                if (extraItems == null || extraItems.Count == 0)
                {
                    Logger.Debug("GetItemRosterReceivingLootShare: no extra items to add.");
                    return;
                }

                Logger.Info("MapEvent loot patch (GetItemRosterReceivingLootShare) adding " + extraItems.Count + " items to player loot roster.");

                foreach (var equipmentElement in extraItems)
                {
                    if (equipmentElement.Item == null || equipmentElement.IsEmpty)
                    {
                        continue;
                    }

                    __result.AddToCounts(equipmentElement, 1);
                }

                Logger.Debug("Player loot roster now has " + __result.Count + " entries after KWYD injection (GetItemRosterReceivingLootShare).");
            }
            catch (Exception)
            {
                // Do not let errors here break the loot screen.
            }
        }
    }
}
