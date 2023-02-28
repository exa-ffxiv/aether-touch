using AetherTouch.App.Common;
using Buttplug.Client;
using Dalamud.Game.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AetherTouch.App.Triggers
{
    public class TriggerService
    {
        private Plugin plugin { get; init; }
        private ButtplugClient client { get; init; }

        private Regex combatRegex = new(@"[0-9]+");

        public TriggerService(Plugin plugin, ButtplugClient cleint)
        { 
            this.plugin = plugin;
            this.client = cleint;
        }

        public void ProcessTextTriggers(XivChatType type, string sender, string message)
        {
            var triggers = plugin.Configuration.GetSortedActiveTriggers();
            Logger.Info($"mainCount={plugin.Configuration.Triggers.Count()} sortedActiveCount={triggers.Count}");
            if (triggers.Count == 0) return;
            foreach (var trigger in triggers) 
            {
                Logger.Info($"Processing trigger={trigger.Name} type={trigger.chatType}");
                if (chatTypeMatch(type, trigger.chatType)
                    )
                {
                    // TODO: check regex for message and sender
                    Logger.Info("Trigger triggered.");
                }
            }
        }

        private bool chatTypeMatch(XivChatType xivType, ChatTypes atType)
        {
            switch (atType)
            {
                case ChatTypes.Any: return true;
                case ChatTypes.Say: return xivType == XivChatType.Say;
                case ChatTypes.Party: return xivType == XivChatType.Party;
                case ChatTypes.Combat: return combatRegex.IsMatch(xivType.ToString());
                case ChatTypes.TellIncoming: return xivType == XivChatType.TellIncoming;
                case ChatTypes.TellOutgoing: return xivType == XivChatType.TellOutgoing;
                default: return false;
            }
        }
    }
}
