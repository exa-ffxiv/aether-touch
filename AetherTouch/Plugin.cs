using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using AetherTouch.App;
using Dalamud.Game.ClientState;
using Dalamud.Game.Network;
using Dalamud.Game.ClientState.Party;
using Lumina.Excel.GeneratedSheets;
using AetherTouch.App.Common;
using Dalamud.Plugin.Services;

namespace AetherTouch
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Aether Touch";
        private const string SettingsCommand = "/aethertouch";

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("AetherTouch");
        public IChatGui dalaChat { get; init; }
        public IClientState ClientState { get; init; }

        private ATApp app { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager,
            [RequiredVersion("1.0")] IChatGui dalaChat,
            [RequiredVersion("1.0")] IClientState clientState)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.ClientState = clientState;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            this.CommandManager.AddHandler(SettingsCommand, new CommandInfo(OnSettingsCommand)
            {
                HelpMessage = "Open the Aether Touch configuration window"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawPluginUI;

            this.app = new ATApp(this, dalaChat);
            
            WindowSystem.AddWindow(app.PluginUI);
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            this.CommandManager.RemoveHandler(SettingsCommand);
            this.app.Dispose();
        }

        private void OnSettingsCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            //MainWindow.IsOpen = true;
            this.DrawPluginUI();
        }

        public void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawPluginUI()
        {
            app.PluginUI.IsOpen = true;
        }
    }
}
