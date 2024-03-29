using AetherTouch.App.Common;
using AetherTouch.App.Patterns;
using AetherTouch.App.Triggers;
using Buttplug.Client;
using Buttplug.Client.Connectors.WebsocketConnector;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TextCopy;

namespace AetherTouch.App.Windows
{
    public class PluginUI: Window, IDisposable
    {
        private Plugin plugin { get; init; }
        private ButtplugClient client { get; init; }
        private ATApp app { get; init; }

        private string newTriggerName = string.Empty;
        private TriggerType newTriggerType = TriggerType.Regex;
        private Trigger? selectedTrigger = null;
        private Guid selectedTriggerId = Guid.Empty;

        private string newPatternName = string.Empty;
        private Pattern? selectedPattern = null;
        private Guid selectedPatternId = Guid.Empty;

        private string patternSearch = string.Empty;

        private string triggerSearch = string.Empty;
        private string triggerPatternSearch = string.Empty;

        public PluginUI(Plugin plugin, ButtplugClient client, ATApp app): base(
            "Aether Touch Configuration",
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.Size = new Vector2(1000, 400);
            this.SizeCondition = ImGuiCond.Appearing;

            this.plugin = plugin;
            this.client = client;
            this.app = app;
        }

        public void Dispose() 
        {
            
        }

        public override void Draw()
        {
            if (ImGui.BeginTabBar("##TabBar", ImGuiTabBarFlags.None))
            {
                if (ImGui.BeginTabItem("Connect"))
                {
                    DrawConnect();
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Devices"))
                {
                    DrawDevices();
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Patterns"))
                {
                    DrawPatterns();
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Triggers"))
                {
                    DrawTriggers();
                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }
        }

        public void DrawConnect()
        {
            var serverAddress = this.plugin.Configuration.ButtplugIOAddress;
            ImGui.SetNextItemWidth(150);
            if (ImGui.InputText("##serverAddress", ref serverAddress, 99))
            {
                this.plugin.Configuration.ButtplugIOAddress = serverAddress;
                this.plugin.Configuration.Save();
            }

            ImGui.SameLine();
            var serverPort = this.plugin.Configuration.ButtplugIOPort;
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputInt("##serverPort", ref serverPort, 10))
            {
                this.plugin.Configuration.ButtplugIOPort = serverPort;
                this.plugin.Configuration.Save();
            }

            ImGui.SetNextItemWidth(200);
            if (app.ClientConnecting)
            {
                ImGui.BeginDisabled();
            }
            if (client.Connected)
            {
                if (ImGui.Button("Disconnect"))
                {
                    app.DisconnectButtplugIO();
                }
            }
            else
            {
                if (ImGui.Button("Connect"))
                {
                    app.ConnectButtplugIO();
                }
            }
            if (app.ClientConnecting)
            {
                ImGui.EndDisabled();
            }
            ImGui.SameLine();
            bool autoConnect = plugin.Configuration.AutoConnect;
            if (ImGui.Checkbox("Auto Connect", ref autoConnect))
            {
                plugin.Configuration.AutoConnect = autoConnect;
                plugin.Configuration.Save();
                if (!client.Connected && plugin.Configuration.AutoConnect)
                {
                    app.ConnectButtplugIO();
                }
            }
            double minVibe = plugin.Configuration.MinimumVibe;
            if (ImGui.InputDouble("Minimum Vibe Level", ref minVibe))
            {
                plugin.Configuration.MinimumVibe = minVibe;
                plugin.Configuration.Save();
            }
        }

        private void DrawDevices()
        {
            if (app.client.Connected)
            {
                if (ImGui.Button("Scan for Devices"))
                {
                    app.StartDeviceScan();
                }
            }
            foreach (var device in app.GetDevices())
            {
                if (ImGui.CollapsingHeader($"{device.Name}"))
                {
                    if (ImGui.Button("0"))
                    {
                        device.VibrateAsync(0);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("5"))
                    {
                        device.VibrateAsync(.05);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("25"))
                    {
                        device.VibrateAsync(.25);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("50"))
                    {
                        device.VibrateAsync(.5);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("75"))
                    {
                        device.VibrateAsync(.75);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("100"))
                    {
                        device.VibrateAsync(1);
                    }
                }
            }
        }

        public void DrawPatterns()
        {
            if (ImGui.BeginChild("##PatternsList", new Vector2(200, -ImGui.GetFrameHeightWithSpacing()), true))
            {
                ImGui.SetNextItemWidth(150);
                ImGui.InputTextWithHint("##newPatternName", "New Pattern Name...", ref newPatternName, 500);
                ImGui.SameLine();
                if (ImGui.Button("+", new Vector2(23, 23)))
                {
                    if (!newPatternName.IsNullOrWhitespace())
                    {
                        var tempPattern = new Pattern(newPatternName);
                        plugin.Configuration.Patterns.Add(tempPattern.Id, tempPattern);
                        plugin.Configuration.Save();
                        newPatternName = "";
                    }
                }
                ImGui.SetNextItemWidth(180);
                ImGui.InputTextWithHint("##patternSearch", "Filter...", ref patternSearch, 500);
                ImGui.Spacing();

                var patternRegex = new Regex(patternSearch, RegexOptions.IgnoreCase);
                foreach (var pattern in plugin.Configuration.Patterns.Values)
                {
                    if (!patternRegex.IsMatch(pattern.Name)) continue;
                    if (ImGui.Selectable($"{pattern.Name}###{pattern.Id}", pattern.Id == selectedPatternId))
                    {
                        selectedPattern = pattern;
                        selectedPatternId = pattern.Id;
                    }
                }

                ImGui.EndChild();
            }

            ImGui.SameLine();
            if (ImGui.BeginChild("##SelectedPattern", new Vector2(0, -ImGui.GetFrameHeightWithSpacing()), true))
            {
                if (selectedPattern != null)
                {
                    ImGui.Text(selectedPattern.Id.ToString());
                    ImGui.SameLine();
                    if (ImGui.Button("Copy ID"))
                    {
                        ClipboardService.SetText(selectedPattern.Id.ToString());
                    }
                    if (ImGui.InputText("Name", ref selectedPattern.Name, 500))
                    {
                        plugin.Configuration.Patterns[selectedPatternId] = selectedPattern;
                        plugin.Configuration.Save();
                    }
                    if (ImGui.InputText("Pattern", ref selectedPattern.PatternText, 1000000, ImGuiInputTextFlags.CharsNoBlank))
                    {
                        plugin.Configuration.Patterns[selectedPatternId] = selectedPattern;
                        plugin.Configuration.Save();
                    }
                    if (ImGui.Button("Delete"))
                    {
                        var temp = selectedPattern;
                        selectedPattern = null;
                        plugin.Configuration.Patterns.Remove(temp.Id);
                    }
                }

                ImGui.EndChild();
            }
        }

        public void DrawTriggers()
        {
            if (ImGui.BeginChild("##TriggersList", new Vector2(200, -ImGui.GetFrameHeightWithSpacing()), true))
            {
                ImGui.SetNextItemWidth(150);
                ImGui.InputTextWithHint("##newTriggerName", "New Trigger Name...", ref newTriggerName, 500);
                if (ImGui.BeginCombo("##newTriggerType", newTriggerType.ToString()))
                {
                    if (ImGui.Selectable("Regex", newTriggerType == TriggerType.Regex))
                    {
                        newTriggerType = TriggerType.Regex;
                    }
                    if (ImGui.Selectable("Spell", newTriggerType == TriggerType.Spell))
                    {
                        newTriggerType = TriggerType.Spell;
                    }
                    ImGui.EndCombo();
                }
                ImGui.SameLine();
                if (ImGui.Button("+", new Vector2(23,23)))
                {
                    if (!newTriggerName.IsNullOrWhitespace())
                    {
                        var tempTrigger = new Trigger(newTriggerName);
                        if (newTriggerType == TriggerType.Spell)
                        {
                            tempTrigger.triggerType = newTriggerType;
                            tempTrigger.chatType = ChatTypes.Combat;
                        }
                        plugin.Configuration.Triggers.Add(tempTrigger.Id, tempTrigger);
                        plugin.Configuration.Save();
                        newTriggerName = "";
                        newTriggerType = TriggerType.Regex;
                    }
                }

                ImGui.SetNextItemWidth(180);
                ImGui.InputTextWithHint("##triggerSearch", "Filter...", ref triggerSearch, 500);
                ImGui.Spacing();

                var triggerRegex = new Regex(triggerSearch, RegexOptions.IgnoreCase);
                foreach (var trigPair in plugin.Configuration.Triggers)
                {
                    if (!triggerRegex.IsMatch(trigPair.Value.Name)) continue;
                    if (ImGui.Selectable($"{trigPair.Value.Name}###{trigPair.Value.Id}", trigPair.Value.Id == selectedTriggerId))
                    {
                        selectedTrigger = trigPair.Value;
                        selectedTriggerId = trigPair.Value.Id;
                    }
                }

                ImGui.EndChild();
            }

            ImGui.SameLine();
            if (ImGui.BeginChild("##SelectedTrigger", new Vector2(0, -ImGui.GetFrameHeightWithSpacing()), true))
            {
                if (selectedTrigger != null)
                {
                    if (selectedTrigger.triggerType == TriggerType.Spell)
                    {
                        DisplaySpellTrigger();
                    }
                    else // Regex Trigger
                    {
                        DisplayRegexTrigger();
                    }
                }
                
                ImGui.EndChild();
            }
        }

        private void DisplayRegexTrigger()
        {
            if (selectedTrigger == null) return;

            if (ImGui.InputText("Trigger Name", ref selectedTrigger.Name, 500))
            {
                SaveTrigger();
            }
            // TODO: Pretty up combo box and see about keeping search box at the top
            var patternName = "";
            if (plugin.Configuration.Patterns.TryGetValue(selectedTrigger.patternId, out var p))
            {
                patternName = p.Name;
            }
            else
            {
                patternName = "None";
            }
            if (ImGui.BeginCombo("Pattern", patternName))
            {
                ImGui.InputTextWithHint("##PatternFilter", "Filter...", ref triggerPatternSearch, 1000);
                Regex patternNameRegex = new Regex(triggerPatternSearch, RegexOptions.IgnoreCase);
                foreach (var pattern in plugin.Configuration.Patterns)
                {
                    if (!patternNameRegex.IsMatch(pattern.Value.Name)) continue;
                    if (ImGui.Selectable($"{pattern.Value.Name}###{pattern.Value.Id}", selectedTrigger.patternId == pattern.Value.Id))
                    {
                        triggerPatternSearch = string.Empty;
                        selectedTrigger.patternId = pattern.Value.Id;
                        SaveTrigger();
                    }
                }
                ImGui.EndCombo();
            }
            if (ImGui.InputText("Message Regex", ref selectedTrigger.messageRegex, 10000))
            {
                SaveTrigger();
            }
            if (ImGui.InputText("Sender Regex", ref selectedTrigger.senderRegex, 10000))
            {
                SaveTrigger();
            }
            var chatTypes = Enum.GetNames(typeof(ChatTypes));
            int selectedChatType = (int)selectedTrigger.chatType;
            if (ImGui.Combo("Chat Type", ref selectedChatType, chatTypes, chatTypes.Length))
            {
                selectedTrigger.chatType = (ChatTypes)selectedChatType;
                SaveTrigger();
            }
            if (ImGui.InputInt("Priority", ref selectedTrigger.priority, 1))
            {
                SaveTrigger();
            }
            if (ImGui.Checkbox("Enabled", ref selectedTrigger.enabled))
            {
                SaveTrigger();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("Ignore own messages", ref selectedTrigger.ignoreOwn))
            {
                SaveTrigger();
            }
            if (ImGui.Button("Delete"))
            {
                var temp = selectedTrigger;
                selectedTrigger = null;
                plugin.Configuration.Triggers.Remove(temp.Id);
                plugin.Configuration.Save();
            }
        }

        private void DisplaySpellTrigger()
        {
            if (selectedTrigger == null) return;

            if (ImGui.InputText("Trigger Name", ref selectedTrigger.Name, 500))
            {
                SaveTrigger();
            }
            // TODO: Pretty up combo box and see about keeping search box at the top
            var patternName = "";
            if (plugin.Configuration.Patterns.TryGetValue(selectedTrigger.patternId, out var p))
            {
                patternName = p.Name;
            }
            else
            {
                patternName = "None";
            }
            if (ImGui.BeginCombo("Pattern", patternName))
            {
                ImGui.InputTextWithHint("##PatternFilter", "Filter...", ref triggerPatternSearch, 1000);
                Regex patternNameRegex = new Regex(triggerPatternSearch, RegexOptions.IgnoreCase);
                foreach (var pattern in plugin.Configuration.Patterns)
                {
                    if (!patternNameRegex.IsMatch(pattern.Value.Name)) continue;
                    if (ImGui.Selectable($"{pattern.Value.Name}###{pattern.Value.Id}", selectedTrigger.patternId == pattern.Value.Id))
                    {
                        triggerPatternSearch = string.Empty;
                        selectedTrigger.patternId = pattern.Value.Id;
                        SaveTrigger();
                    }
                }
                ImGui.EndCombo();
            }
            if (ImGui.InputText("Spell or Ability name", ref selectedTrigger.spellName, 10000))
            {
                SetSpellTriggerMessageRegex();
                SaveTrigger();
            }
            if (ImGui.Checkbox("Begin Casting", ref selectedTrigger.onBeginCast))
            {
                SetSpellTriggerMessageRegex();
                SaveTrigger();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("Finish Casting", ref selectedTrigger.onAbilityUsage))
            {
                SetSpellTriggerMessageRegex();
                SaveTrigger();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("Cancel Cast", ref selectedTrigger.onCancelCast))
            {
                SetSpellTriggerMessageRegex();
                SaveTrigger();
            }

            if (ImGui.Button("Delete"))
            {
                var temp = selectedTrigger;
                selectedTrigger = null;
                plugin.Configuration.Triggers.Remove(temp.Id);
                plugin.Configuration.Save();
            }
        }

        private void SetSpellTriggerMessageRegex()
        {
            if (selectedTrigger == null) return;

            StringBuilder sb = new StringBuilder();
            sb.Append("(?:");
            if (selectedTrigger.onBeginCast)
            {
                sb.Append($"You begin casting {selectedTrigger.spellName}|You ready {selectedTrigger.spellName}");
            }
            if (selectedTrigger.onCancelCast)
            {
                if (sb.Length > 3) sb.Append('|');
                sb.Append($"You cancel {selectedTrigger.spellName}");
            }
            if (selectedTrigger.onAbilityUsage)
            {
                if (sb.Length > 3) sb.Append('|');
                sb.Append($"You use {selectedTrigger.spellName}|You cast {selectedTrigger.spellName}");
            }
            sb.Append(")");
            selectedTrigger.messageRegex = sb.ToString();
        }

        private void SaveTrigger()
        {
            if (selectedTrigger == null) return;
            plugin.Configuration.Triggers[selectedTriggerId] = selectedTrigger;
            plugin.Configuration.Save();
        }
    }
}
