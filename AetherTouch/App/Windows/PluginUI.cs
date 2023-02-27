using AetherTouch.App.Triggers;
using Buttplug.Client;
using Buttplug.Client.Connectors.WebsocketConnector;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AetherTouch.App.Windows
{
    public class PluginUI: Window, IDisposable
    {
        private Plugin plugin { get; init; }
        private ButtplugClient client { get; init; }
        private ATApp app { get; init; }

        private string searchText = string.Empty;
        private string newTriggerName = string.Empty;
        private Trigger? selectedTrigger = null;
        private Guid selectedTriggerId = Guid.Empty;

        public PluginUI(Plugin plugin, ButtplugClient client, ATApp app): base(
            "Aether Touch Configuration",
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.Size = new Vector2(400, 400);
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
            ImGui.BeginGroup();
            ImGui.Text("Patterns");
            ImGui.EndGroup();
        }

        public void DrawTriggers()
        {
            if (ImGui.BeginChild("##TriggersList", new Vector2(200, -ImGui.GetFrameHeightWithSpacing()), true))
            {
                ImGui.SetNextItemWidth(150);
                ImGui.InputText("##newTriggerName", ref newTriggerName, 500);
                ImGui.SameLine();
                if (ImGui.Button("+", new Vector2(23,23)))
                {
                    var tempTrigger = new RegexTrigger(newTriggerName);
                    plugin.Configuration.Triggers.Add(tempTrigger.Id, tempTrigger);
                    plugin.Configuration.Save();
                    newTriggerName = "";
                }
                ImGui.Spacing();

                foreach (var trigPair in plugin.Configuration.Triggers)
                {
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
                    if (ImGui.InputText("Name", ref selectedTrigger.Name, 500))
                    {
                        plugin.Configuration.Triggers[selectedTriggerId] = selectedTrigger;
                        plugin.Configuration.Save();
                    }
                }
                
                ImGui.EndChild();
            }
        }

        public void DrawRegexTrigger()
        {

        }
    }
}