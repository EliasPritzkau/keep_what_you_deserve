using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace KeepWhatYouDeserve
{
    public class SubModule : MBSubModuleBase
    {
        private static bool _harmonyInitialized;
        private static bool _loadMessageShown;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            if (_harmonyInitialized)
            {
                return;
            }

            try
            {
                var harmony = new Harmony("mod.keep_what_you_deserve");
                harmony.PatchAll();
                _harmonyInitialized = true;
            }
            catch (Exception)
            {
                // Swallow exceptions to avoid breaking game loading if patching fails.
            }
        }

        protected override void OnGameStart(Game game, IGameStarter starterObject)
        {
            base.OnGameStart(game, starterObject);

            if (_loadMessageShown)
            {
                return;
            }

            if (game != null && game.GameType is Campaign)
            {
                Logger.Info("Keep What You Deserve successfully loaded");
                _loadMessageShown = true;
            }
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);

            if (mission == null)
            {
                return;
            }

            // Only relevant for campaign / sandbox style games.
            if (Game.Current == null || !(Game.Current.GameType is Campaign))
            {
                return;
            }

            mission.AddMissionBehavior(new KeepWhatYouDeserveMissionBehavior());
        }
    }
}
