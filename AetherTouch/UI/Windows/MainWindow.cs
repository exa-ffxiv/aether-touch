using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AetherTouch.UI.Tabs;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Lumina.Text;

namespace AetherTouch.UI.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private readonly ToyManager toyManager;
    private readonly DataManager dataManager;
    private readonly NotificationManager toastGui;
    private readonly IPluginLog log;

    // Tabs
    private readonly PatternTab patternTab;

    // We give this window a hidden ID using ##.
    // The user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, ToyManager toyManager, NotificationManager toastGui, DataManager dataManager, IPluginLog log)
        : base("Aether Touch##MainWindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.toyManager = toyManager;
        this.dataManager = dataManager;
        this.plugin = plugin;
        this.toastGui = toastGui;
        this.log = log;

        patternTab = new PatternTab(dataManager, toyManager);
    }

    public void Dispose() { }

    private unsafe int FilterDigitsOnly(ImGuiInputTextCallbackData* data)
    {
        // Check if the input character is a digit
        char c = (char)data->EventChar;
        if (c < '0' || c > '9')
        {
            // Set EventChar to 0 to drop the keystroke entirely
            data->EventChar = 0;
        }
        return 0;
    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("Main Window Tabs"))
        {
            if (ImGui.BeginTabItem("General"))
            {
                if (ImGui.Button("Connect"))
                {
                    toyManager.connect();
                }
                if (ImGui.Button("Vibe"))
                {
                    toyManager.testVibe();
                }
                if (ImGui.Button("Vibe Pattern"))
                {
                    log.Debug("Starting pattern");
                    var pattern = new Pattern([
                        new Step(500, 25),
                        new Step(2000, 100),
                        new Step(500, 0),
                        new Step(500, 25)
                        ]);
                    toyManager.playPattern(pattern);
                }
                if (ImGui.Button("Disconnect"))
                {
                    toyManager.disconnect();
                }
                ImGui.EndTabItem();
            }

            
            if (ImGui.BeginTabItem("Patterns"))
            {
                patternTab.Draw();
            }

            ImGui.EndTabBar();
        }
    }
}
