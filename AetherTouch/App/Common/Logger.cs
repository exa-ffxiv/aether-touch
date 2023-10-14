using Dalamud.Plugin.Services;

namespace AetherTouch.App.Common
{
    public static class Logger
    {
        private static IPluginLog? Log;

        public static void initialize(IPluginLog log)
        {
            if (Log == null) return;
            Log = log;
        }

        public static void Error(string message)
        {
            if (Log == null) return;
            Log.Error(message);
        }

        public static void Warning(string message)
        {
            if (Log == null) return;
            Log.Warning(message);
        }

        public static void Info(string message)
        {
            if (Log == null) return;
            Log.Information(message);
        }

        public static void Debug(string message)
        {
            if (Log == null) return;
            Log.Debug(message);
        }
    }
}
