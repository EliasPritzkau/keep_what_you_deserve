using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace KeepWhatYouDeserve
{
    internal class KeepWhatYouDeserveMissionBehavior : MissionBehavior
    {
        public override MissionBehaviorType BehaviorType
        {
            get { return MissionBehaviorType.Other; }
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            Logger.RefreshCache();
            PersonalLootManager.BeginTrackingForCurrentMapEvent();
            Logger.Debug("Mission behavior initialized; tracking started (if map event present).");
        }

        public override void OnAgentRemoved(
            Agent affectedAgent,
            Agent affectorAgent,
            AgentState agentState,
            KillingBlow killingBlow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, killingBlow);

            if (affectedAgent == null || affectorAgent == null)
            {
                return;
            }

            // If the player (affectedAgent) is knocked out/killed, mark it immediately regardless of who hit them.
            if (affectedAgent.IsMainAgent && (agentState == AgentState.Unconscious || agentState == AgentState.Killed))
            {
                PersonalLootManager.MarkPlayerKnockedOut();
            }
            else if (affectedAgent.Character != null && affectedAgent.Character.IsHero && (agentState == AgentState.Unconscious || agentState == AgentState.Killed))
            {
                CharacterObject hChar = affectedAgent.Character as CharacterObject;
                if (hChar != null && hChar.HeroObject != null)
                {
                    PersonalLootManager.MarkHeroKnockedOut(hChar.HeroObject);
                }
            }

            var settingsRef = KeepWhatYouDeserveSettings.Instance;
            bool countCompanions = settingsRef != null ? settingsRef.CountCompanions : false;

            bool killerIsPlayer = affectorAgent.IsMainAgent;
            bool killerIsCompanion = false;

            if (!killerIsPlayer && countCompanions)
            {
                var originParty = affectorAgent.Origin != null
                    ? affectorAgent.Origin.BattleCombatant as PartyBase
                    : null;

                bool isHero = affectorAgent.Character != null && affectorAgent.Character.IsHero;
                if (originParty != null && originParty == MobileParty.MainParty.Party && isHero)
                {
                    killerIsCompanion = true;
                }
            }

            if (!killerIsPlayer && !killerIsCompanion)
            {
                return;
            }

            // Only count actual casualties (killed or knocked out).
            if (agentState != AgentState.Killed && agentState != AgentState.Unconscious)
            {
                return;
            }

            // Respect player KO setting: if player is KO and setting disallows loot, skip.
            if (Mission.MainAgent != null && Mission.MainAgent.State == AgentState.Unconscious)
            {
                var settings = KeepWhatYouDeserveSettings.Instance;
                bool allowLootWhenKO = settings != null ? settings.LootWhenPlayerKO : false;
                if (!allowLootWhenKO)
                {
                    Logger.Debug("Player is knocked out and 'Loot when player knocked out' is disabled; skipping kill.");
                    return;
                }
            }

            // Respect friendly/noble filters.
            bool countFriendlies = settingsRef != null ? settingsRef.CountFriendlies : false;
            bool countNobles = settingsRef != null ? settingsRef.CountNobles : true;

            bool isEnemy = affectedAgent.Team != null && affectorAgent.Team != null && affectedAgent.Team.IsEnemyOf(affectorAgent.Team);
            bool isFriendly = !isEnemy;

            if (isFriendly && !countFriendlies)
            {
                Logger.Debug("Friendly target and 'Count friendly kills' disabled; skipping kill.");
                return;
            }

            bool victimIsHero = affectedAgent.Character != null && affectedAgent.Character.IsHero;
            if (victimIsHero && !countNobles)
            {
                Logger.Debug("Victim is noble/hero and 'Count nobles/lords' disabled; skipping kill.");
                return;
            }

            // Only in campaign battles with an active map event.
            if (Game.Current == null || !(Game.Current.GameType is Campaign) || MobileParty.MainParty == null || MobileParty.MainParty.MapEvent == null)
            {
                return;
            }

            Hero killerHero = null;
            if (affectorAgent.IsMainAgent)
            {
                killerHero = Hero.MainHero;
            }
            else
            {
                CharacterObject kChar = affectorAgent.Character as CharacterObject;
                if (kChar != null && kChar.HeroObject != null)
                {
                    killerHero = kChar.HeroObject;
                }
            }
            int addedCount = PersonalLootManager.RegisterKill(affectedAgent, killerHero, killerIsPlayer);
            Logger.Info("Recorded kill on " + affectedAgent.Name + "; collected " + addedCount + " equipped items.");
        }
    }
}
