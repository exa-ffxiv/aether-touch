using AetherTouch.App.Common;
using AetherTouch.App.Patterns;
using Buttplug.Client;
using Dalamud.Game.Text;
using Dalamud.Utility;
using Lumina.Data.Parsing.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public TriggerService(Plugin plugin, ButtplugClient client)
        { 
            this.plugin = plugin;
            this.client = client;
        }

        public void ProcessTextTriggers(XivChatType type, string sender, string message)
        {
            var triggers = plugin.Configuration.GetSortedActiveTriggers();
            Logger.Debug($"Recieved message. type={type} sender={sender} message={message}");
            if (triggers.Count == 0) return;
            foreach (var trigger in triggers) 
            {
                Logger.Debug($"Processing trigger={trigger.Name} type={trigger.chatType}");
                if (chatTypeMatch(type, trigger.chatType) &&
                    senderMatch(sender, trigger.senderRegex) &&
                    shouldOverrideRunningTrigger(trigger))
                {
                    // TODO: Expand messageMatchResult with capture group data.
                    var messageMatchResult = messageMatch(message, trigger.messageRegex);
                    if (!messageMatchResult.isMatch) continue;
                    if (client == null || !client.Connected || client.Devices.Length == 0)
                    {
                        Logger.Warning($"Triggered but client is not setup. connected={client?.Connected} deviceCount={client?.Devices.Length}");
                        return;
                    }
                    Logger.Debug($"Trigger triggered. name={trigger.Name}");
                    RunTrigger(trigger, messageMatchResult);
                    break;
                }
            }
        }

        private bool chatTypeMatch(XivChatType xivType, ChatTypes atType)
        {
            var t = atType switch
            {
                ChatTypes.Any => true,
                ChatTypes.Say => xivType == XivChatType.Say,
                ChatTypes.Party => xivType == XivChatType.Party,
                ChatTypes.Combat => combatRegex.IsMatch(xivType.ToString()),
                ChatTypes.TellIncoming => xivType == XivChatType.TellIncoming,
                ChatTypes.TellOutgoing => xivType == XivChatType.TellOutgoing,
                _ => false,
            };
            Logger.Debug($"ChatType compare. result={t} xivType={xivType} atType={atType}");
            return t;
        }

        private bool senderMatch(string sender, string senderRegex)
        {
            if (senderRegex.IsNullOrWhitespace()) return true;
            var t =  new Regex(senderRegex).IsMatch(sender);
            Logger.Debug($"Sender compare. result={t} sender='{sender}' reges='{senderRegex}'");
            return t;
        }

        private MessageMatchResult messageMatch(string message, string messageRegex)
        {
            var matches = new Regex(messageRegex).Matches(message);
            Logger.Debug($"Message compare. result={matches.Count != 0} message={message} regex={messageRegex}");
            if (matches.Count == 0) return new MessageMatchResult(false);
            var intensity = "";
            var duration = "";
            foreach (var group in matches[0].Groups.Values)
            {
                if (group.Name == "intensity") intensity = group.Value;
                if (group.Name == "duration") duration = group.Value;
            }

            Logger.Debug($"Regex group results intensity={intensity} duration={duration}");
            return new MessageMatchResult(true, intensity, duration);
        }

        private bool shouldOverrideRunningTrigger(Trigger newTrigger)
        {
            // No running trigger, new one can start.
            if (activeTask == null || currentRunningTrigger == null) return true;
            // Running trigger is the same as new trigger, restart pattern.
            if (currentRunningTrigger.Id.Equals(newTrigger.Id)) return true;
            // Run new trigger if higher priority than running trigger.
            return newTrigger.priority > currentRunningTrigger.priority;
        }

        // TODO: A lot more error handling
        // TODO: Let different devices get different patterns?
        private void RunTrigger(Trigger trigger, MessageMatchResult messageMatchResult)
        {
            if (activeTask != null)
            {
                cancelTokenSource.Cancel();
                activeTask = null;
            }
            if (plugin.Configuration.Patterns.TryGetValue(trigger.patternId, out var pattern))
            {
                cancelTokenSource = new CancellationTokenSource();
                currentRunningTrigger = trigger;
                activeTask = Task.Run(() => TriggerTask(cancelTokenSource.Token, pattern, messageMatchResult));
            }
        }

        private async Task TriggerTask(CancellationToken cancelToken, Pattern pattern, MessageMatchResult messageMatchResult)
        {
            var patternParts = pattern.PatternText.Split(',').Reverse();
            var patternStack = new Stack<string>();
            foreach (var part in patternParts) { patternStack.Push(part); }
            while (patternStack.Count > 0)
            {
                var part = patternStack.Pop();
                var splitVals = part.Split(":");
                if (!IsPatternPartValid(pattern, part, splitVals)) continue;

                var intensityString = splitVals[0];
                var durationString = splitVals[1];
                if (intensityString.Contains("{intensity}")) intensityString = messageMatchResult.intensity;
                if (durationString.Contains("{duration}")) durationString = messageMatchResult.duration;

                if (IsPartInfinite(intensityString, durationString)) return;

                var intensity = double.Parse(intensityString);
                var duration = int.Parse(durationString);
                Logger.Debug($"Starting pattern part. intensity={intensity / 100} duration={duration}");
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
            currentRunningTrigger = null;
        }

        private bool IsPatternPartValid(Pattern pattern, string part, string[] parts)
        {
            if (parts.Length != 2)
            {
                Logger.Warning($"Invalid pattern part length. patternName={pattern.Name} part={part}");
                return false;
            }

            return true;
        }

        private bool IsPartInfinite(string intensityStr, string durationStr)
        {
            if (durationStr == "~")
            {
                currentRunningTrigger = null;
                activeTask = null;
                var intensity = double.Parse(intensityStr);
                foreach (var device in client.Devices)
                {
                    device.VibrateAsync(intensity / 100);
                }
                return true;
            }
            return false;
        }

        //private bool IsPartInfinite(string[] parts)
        //{
        //    if (parts[1] == "~")
        //    {
        //        currentRunningTrigger = null;
        //        activeTask = null;
        //        var intensity = double.Parse(parts[0]);
        //        foreach (var device in client.Devices)
        //        {
        //            device.VibrateAsync(intensity / 100);
        //        }
        //        return true;
        //    }
        //    return false;
        //}
    }
}
