using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AetherTouch.App.Common
{
    public static class Logger
    {
        public static void Error(string message)
        {
            Dalamud.Logging.PluginLog.Error(message);
        }

        public static void Warning(string message)
        {
            Dalamud.Logging.PluginLog.Warning(message);
        }

        public static void Info(string message)
        {
            Dalamud.Logging.PluginLog.Information(message);
        }

        public static void Debug(string message)
        {
            Dalamud.Logging.PluginLog.Debug(message);
        }
    }
}
