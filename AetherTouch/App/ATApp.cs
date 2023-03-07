using AetherTouch.App.Common;
using AetherTouch.App.Triggers;
using AetherTouch.App.Windows;
using Buttplug.Client;
using Buttplug.Client.Connectors.WebsocketConnector;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AetherTouch.App
{
    public class ATApp: IDisposable
    {
        private Plugin Plugin { get; init; }
        private ChatGui? DalaChat { get; init; }
        public ButtplugClient client { get; init; }
        public TriggerService triggerService { get; init; }

        public PluginUI PluginUI { get; init; }

        public bool ClientConnecting { get; set; } = false;

        public ATApp(Plugin plugin, ChatGui? dalaChat)
        {
            Plugin = plugin;
            DalaChat = dalaChat;
            if (DalaChat == null)
            {
                Dalamud.Logging.PluginLog.Error("DalaChat is null. Unable to setup chat handler.");
            }
            else
            {
                DalaChat.ChatMessage += DalaChat_ChatMessage;
            }
            
            client = new ButtplugClient("Aether Touch Client");
            PluginUI = new PluginUI(plugin, client, this);
            triggerService = new TriggerService(plugin, client, this);

            if (client != null && !client.Connected && plugin.Configuration.AutoConnect)
            {
                ConnectButtplugIO();
            }
        }

        private void DalaChat_ChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            triggerService.ProcessTextTriggers(type, sender.TextValue, message.TextValue);
        }

        public async void Dispose()
        {
            if (DalaChat != null)
            {
                DalaChat.ChatMessage -= DalaChat_ChatMessage;
            }

            if (client.Connected) await client.DisconnectAsync();
        }

        public void ConnectButtplugIO()
        {
            var address = this.Plugin.Configuration.ButtplugIOAddress;
            var port = this.Plugin.Configuration.ButtplugIOPort;
            var uri = $"ws://{address}:{port}";

            try
            {
                if (client != null && !client.Connected)
                {
                    ClientConnecting = true;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await client.ConnectAsync(new ButtplugWebsocketConnector(new Uri(uri)));
                        }
                        catch (Exception ex)
                        {
                            Dalamud.Logging.PluginLog.Error($"Failed to connect to server. Uri={uri} Exception={ex}");
                        }
                        finally
                        {
                            ClientConnecting = false;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Dalamud.Logging.PluginLog.Error($"Failed to connect to server. Uri={uri} Exception={ex}");
            }
        }

        public void DisconnectButtplugIO()
        {
            try
            {
                if (client != null && client.Connected)
                {
                    client.DisconnectAsync();
                }
            }
            catch (Exception ex)
            {
                Dalamud.Logging.PluginLog.Error($"Failed to disconnect to server. Exception={ex}");
            }
        }

        public void StartDeviceScan()
        {
            client?.StartScanningAsync();
        }

        public void StopDeviceScan()
        {
            client?.StopScanningAsync();
        }

        public ButtplugClientDevice[] GetDevices()
        {
            if (client == null || !client.Connected) return Array.Empty<ButtplugClientDevice>();

            return client.Devices;
        }

        public async void VibeAllDevices(double intensityPercent)
        {
            foreach (var device in client.Devices)
            {
                if (device == null) continue;
                device.VibrateAsync(intensityPercent / 100);
            }
        }
    }
}
