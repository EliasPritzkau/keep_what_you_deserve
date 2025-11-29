using System;
using TaleWorlds.Library;

namespace KeepWhatYouDeserve
{
    internal static class Logger
    {
        private const string Prefix = "[KWYD]";
        private static bool _cachedEnabled;
        private static bool _cacheInitialized;

        public static void Info(string message)
        {
            if (!IsEnabled())
            {
                return;
            }

            try
            {
                InformationManager.DisplayMessage(new InformationMessage(Prefix + " " + message));
            }
            catch
            {
                // Swallow to avoid breaking gameplay if UI is unavailable.
            }
        }

        public static void Debug(string message)
        {
            if (!IsEnabled())
            {
                return;
            }

            try
            {
#if DEBUG
                InformationManager.DisplayMessage(new InformationMessage(Prefix + " " + message));
#else
                TaleWorlds.Library.Debug.Print(Prefix + " " + message, 0, TaleWorlds.Library.Debug.DebugColor.Cyan);
#endif
            }
            catch
            {
                // Ignore logging failures.
            }
        }

        private static bool IsEnabled()
        {
            try
            {
                if (!_cacheInitialized)
                {
                    RefreshCache();
                }

                return _cachedEnabled;
            }
            catch
            {
                return false;
            }
        }

        public static void RefreshCache()
        {
            try
            {
                var settings = KeepWhatYouDeserveSettings.Instance;
                if (settings == null)
                {
                    _cachedEnabled = false;
                }
                else
                {
                    _cachedEnabled = settings.EnableInGameLogging;
                }
            }
            catch
            {
                _cachedEnabled = false;
            }

            _cacheInitialized = true;
        }
    }
}
