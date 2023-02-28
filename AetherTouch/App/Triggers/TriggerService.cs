using AetherTouch.App.Common;
using AetherTouch.App.Patterns;
using Buttplug.Client;
using Dalamud.Game.Text;
using Dalamud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AetherTouch.App.Triggers
{
    public class TriggerService
    {
        private Plugin plugin { get; init; }
        private ButtplugClient client { get; init; }

        private Regex combatRegex = new(@"[0-9]+");

        private Trigger? currentRunningTrigger = null;
        private Task? activeTask = null;
        private CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        private CancellationToken cancelToken;

        public TriggerService(Plugin plugin, ButtplugClient client)
        { 
            this.plugin = plugin;
            this.client = client;
        }

        public void ProcessTextTriggers(XivChatType type, string sender, string message)
        {
            var triggers = plugin.Configuration.GetSortedActiveTriggers();
            //Logger.Info($"mainCount={plugin.Configuration.Triggers.Count()} sortedActiveCount={triggers.Count}");
            if (triggers.Count == 0) return;
            foreach (var trigger in triggers) 
            {
                //Logger.Info($"Processing trigger={trigger.Name} type={trigger.chatType}");
                if (chatTypeMatch(type, trigger.chatType) &&
                    senderMatch(sender, trigger.senderRegex) &&
                    messageMatch(message, trigger.messageRegex) &&
                    shouldOverrideRunningTrigger(trigger))
                {
                    if (client == null || !client.Connected || client.Devices.Length == 0)
                    {
                        Logger.Warning($"Triggered but client is not setup. connected={client?.Connected} deviceCount={client?.Devices.Length}");
                        return;
                    }
                    Logger.Info("Trigger triggered.");
                    RunTrigger(trigger);
                    break;
                }
            }
        }

        private bool chatTypeMatch(XivChatType xivType, ChatTypes atType)
        {
            return atType switch
            {
                ChatTypes.Any => true,
                ChatTypes.Say => xivType == XivChatType.Say,
                ChatTypes.Party => xivType == XivChatType.Party,
                ChatTypes.Combat => combatRegex.IsMatch(xivType.ToString()),
                ChatTypes.TellIncoming => xivType == XivChatType.TellIncoming,
                ChatTypes.TellOutgoing => xivType == XivChatType.TellOutgoing,
                _ => false,
            };
        }

        private bool senderMatch(string sender, string senderRegex)
        {
            if (senderRegex.IsNullOrWhitespace()) return true;
            return new Regex(senderRegex).IsMatch(sender);
        }

        private bool messageMatch(string message, string messageRegex)
        {
            if (messageRegex.IsNullOrWhitespace()) return true;
            return new Regex(messageRegex).IsMatch(message);
        }

        private bool shouldOverrideRunningTrigger(Trigger newTrigger)
        {
            // No running trigger, new one can start.
            if (currentRunningTrigger == null) return true;
            // Running trigger is the same as new trigger, restart pattern.
            if (currentRunningTrigger.Id.Equals(newTrigger.Id)) return true;
            // Run new trigger if higher priority than running trigger.
            return newTrigger.priority > currentRunningTrigger.priority;
        }

        // TODO: A lot more error handling
        // TODO: Let different devices get different patterns?
        // TODO: Figure out how to properly cancel a task/thread when a new trigger is triggered.
        private void RunTrigger(Trigger trigger)
        {
            if (activeTask != null &&  !activeTask.IsCompleted)
            {
                cancelTokenSource.Cancel();
            }
            cancelToken = cancelTokenSource.Token;
            if (Guid.TryParse(trigger.patternId, out var patternGuid) && plugin.Configuration.Patterns.TryGetValue(patternGuid, out var pattern))
            {
                var patternParts = pattern.PatternText.Split(',').Reverse();

                var task = Task.Run(async () =>
                {
                    var patternStack = new Stack<string>();
                    foreach (var part in patternParts) { patternStack.Push(part); }
                    foreach (var part in patternStack)
                    {
                        var splitVals = part.Split(":");
                        var intensity = double.Parse(splitVals[0]);
                        var duration = int.Parse(splitVals[1]);
                        Logger.Info($"Starting pattern part. intensity={intensity / 100} duration={duration}");
                        foreach (var device in client.Devices)
                        {
                            device.VibrateAsync(intensity / 100);
                        }
                        await Task.Delay(duration);
                        if (cancelToken.IsCancellationRequested) return;
                    }

                    foreach (var device in client.Devices)
                    {
                        device.VibrateAsync(0);
                    }
                });
            }
        }
    }
}
