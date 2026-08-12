using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using AetherTouch.Windows;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AetherTouch;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static INotificationManager DNotificationManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    private const string CommandName = "/aetouch";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("AetherTouch");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    private readonly NotificationManager notificationManager;
    private readonly DataManager dataManager;
    private readonly ToyManager toyClient;

    public Plugin()
    {
        // Initialize Aethertouch classes
        notificationManager = new NotificationManager(DNotificationManager);
        dataManager = new DataManager(PluginInterface, Log);
        dataManager.Initialize();
        toyClient = new ToyManager(Log, notificationManager);

        // From sample plugin
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // You might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, toyClient, notificationManager, Log);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        ChatGui.ChatMessage += ChatGui_ChatMessage;

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        // Tell the UI system that we want our windows to be drawn through the window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // This adds a button to the plugin installer entry of this plugin which allows
        // toggling the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        // Adds another button doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
    }

    private void ChatGui_ChatMessage(Dalamud.Game.Chat.IHandleableChatMessage message)
    {
        var text = message.OriginalMessage.ExtractText();
        var match = Regex.Match(text, "Test", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            toyClient.playPattern(new Pattern([
                new Step(500, 25),
                        new Step(2000, 100),
                        new Step(500, 0),
                        new Step(500, 25)
                ]));
        }
        //var filteredKinds = new List<string>() { "Action", "GainBuff", "LoseBuff", "Item" };
        //if (filteredKinds.Contains(message.LogKind.ToString()))
        //    return;

        //Log.Info($"""

        //    Text:   {message.OriginalMessage.ExtractText()}
        //    Sender: {message.OriginalSender}
        //    PayCount: {string.Join(", ", message.Sender.Payloads)}
        //    Kind:   {message.LogKind}
        //    """);
    }

    public void Dispose()
    {
        // Unregister all actions to not leak anything during disposal of plugin
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
        toyClient.disconnect();
    }

    private void OnCommand(string command, string args)
    {
        // In response to the slash command, toggle the display status of our main ui
        MainWindow.Toggle();
    }
    
    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}
