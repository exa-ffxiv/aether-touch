using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Lumina.Text;

namespace AetherTouch.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private readonly ToyManager toyManager;
    private readonly DataManager dataManager;
    private readonly NotificationManager toastGui;
    private readonly IPluginLog log;

    private int selectedPatternIndex = -1;

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
                if (ImGui.BeginTable("Pattern Table", 2))
                {
                    ImGui.TableSetupColumn("PatternListColumn", ImGuiTableColumnFlags.WidthFixed, 210f);
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, -1f);

                    ImGui.TableSetColumnIndex(0);
                    if (ImGui.Button("New Pattern"))
                    {
                        Pattern p = new Pattern([
                                new Step(500, 25),
                                        new Step(2000, 100),
                                        new Step(500, 0),
                                        new Step(500, 25)
                                ], "Testing 1");
                        dataManager.SavePattern(p);
                        selectedPatternIndex = dataManager.Patterns.IndexOf(p);
                    }
                    if (ImGui.BeginListBox("##PaternList", new Vector2(200f, ImGui.GetWindowHeight()-85)))
                    {
                        for (var i = 0; i < dataManager.Patterns.Count; i++)
                        {
                            var pattern = dataManager.Patterns[i];
                            bool isSelected = selectedPatternIndex == i;

                            string label = $"{pattern.Name}##{pattern.Id}";
                            if (ImGui.Selectable(label, isSelected))
                            {
                                selectedPatternIndex = i;
                            }

                            if (isSelected)
                            {
                                ImGui.SetItemDefaultFocus();
                            }
                        }
                        ImGui.EndListBox();
                    }

                    ImGui.TableSetColumnIndex(1);
                    if (selectedPatternIndex > -1 && selectedPatternIndex < dataManager.Patterns.Count)
                    {
                        var selected = dataManager.Patterns[selectedPatternIndex];
                        var name = selected.Name;
                        var id = selected.Id.ToString();

                        // Pattern details
                        ImGui.SetNextItemWidth(250f);
                        ImGui.InputText("Id", ref id, 100, ImGuiInputTextFlags.ReadOnly);
                        ImGui.SetNextItemWidth(250f);
                        if (ImGui.InputText("Name", ref name, 40))
                        {
                            selected.Name = name;
                            dataManager.SavePattern(selected);
                        }

                        ImGui.BeginChild("PatternDetailsChild", new Vector2(0, ImGui.GetWindowHeight() - 150), true);
                        ImGui.SetCursorScreenPos(ImGui.GetCursorScreenPos() + new Vector2(0.0f, 6.0f));

                        // Steps
                        for (var i = 0; i < selected.Steps.Count; i++)
                        {
                            var step = selected.Steps[i];
                            var intensity = step.Intensity;
                            var duration = step.Duration;
                            var minPos = ImGui.GetCursorScreenPos();

                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 6.0f);
                            ImGui.BeginGroup();
                            if (i == 0) ImGui.BeginDisabled();
                            if (ImGui.Button("^"))
                            {
                                var temp = selected.Steps[i - 1];
                                selected.Steps[i - 1] = step;
                                selected.Steps[i] = temp;
                            }
                            if (i == 0) ImGui.EndDisabled();
                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(100f);
                            if (ImGui.InputInt($"Duration (Milliseconds)##{i}", ref duration, 10, 100))
                            {
                                var newStep = new Step(duration, step.Intensity);
                                selected.Steps[i] = newStep;
                                dataManager.SavePattern(selected);
                            }
                            ImGui.SameLine();
                            if (ImGui.Button($"Delete##{i}"))
                            {
                                selected.Steps.RemoveAt(i);
                                dataManager.SavePattern(selected);
                            }

                            if (i == selected.Steps.Count - 1) ImGui.BeginDisabled();
                            if (ImGui.Button("v"))
                            {
                                var temp = selected.Steps[i + 1];
                                selected.Steps[i + 1] = step;
                                selected.Steps[i] = temp;
                            }
                            if (i == selected.Steps.Count - 1) ImGui.EndDisabled();
                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(100f);
                            if (ImGui.InputInt($"Intensity (0-100)##{i}", ref intensity, 1, 10))
                            {
                                var newStep = new Step(step.Duration, intensity);
                                selected.Steps[i] = newStep;
                                dataManager.SavePattern(selected);
                            }
                            ImGui.EndGroup();

                            var rightPadding = ImGui.GetScrollMaxY() > 0.0f ? 20.0f : 5.0f;
                            // Draw a border around the group
                            var maxPos = ImGui.GetItemRectMax();
                            minPos.X -= 0.0f;
                            minPos.Y -= 4.0f;
                            maxPos.X = ImGui.GetWindowPos().X + ImGui.GetWindowSize().X - rightPadding;
                            maxPos.Y += 4.0f;
                            uint borderColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                            ImGui.GetWindowDrawList().AddRect(minPos, maxPos, borderColor, 0.0f, ImDrawFlags.RoundCornersAll, 1.0f);
                            // Add some spacing between the groups
                            ImGui.SetCursorScreenPos(ImGui.GetCursorScreenPos() + new Vector2(0.0f, 6.0f));
                        }
                        ImGui.EndChild();

                        if (ImGui.Button("Add Step"))
                        {
                            var newStep = new Step(500, 25);
                            selected.Steps.Add(newStep);
                            dataManager.SavePattern(selected);
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Test Pattern"))
                        {
                            toyManager.playPattern(selected);
                        }
                    }

                    ImGui.EndTable();
                }

            }

            ImGui.EndTabBar();
        }
        //ImGui.Text($"The random config bool is {plugin.Configuration.SomePropertyToBeSavedAndWithADefault}");

        //if (ImGui.Button("Show Settings"))
        //{
        //    plugin.ToggleConfigUi();
        //}

        //ImGui.Spacing();

        //// Normally a BeginChild() would have to be followed by an unconditional EndChild(),
        //// ImRaii takes care of this after the scope ends.
        //// This works for all ImGui functions that require specific handling, examples are BeginTable() or Indent().
        //using (var child = ImRaii.Child("SomeChildWithAScrollbar", Vector2.Zero, true))
        //{
        //    // Check if this child is drawing
        //    if (child.Success)
        //    {
        //        ImGui.Text("Have a goat:");
        //        var goatImage = Plugin.TextureProvider.GetFromFile(goatImagePath).GetWrapOrDefault();
        //        if (goatImage != null)
        //        {
        //            using (ImRaii.PushIndent(55f))
        //            {
        //                ImGui.Image(goatImage.Handle, goatImage.Size);
        //            }
        //        }
        //        else
        //        {
        //            ImGui.Text("Image not found.");
        //        }

        //        ImGuiHelpers.ScaledDummy(20.0f);

        //        // Example for other services that Dalamud provides.
        //        // PlayerState provides a wrapper filled with information about the player character.

        //        var playerState = Plugin.PlayerState;
        //        if (!playerState.IsLoaded)
        //        {
        //            ImGui.Text("Our local player is currently not logged in.");
        //            return;
        //        }
                
        //        if (!playerState.ClassJob.IsValid)
        //        {
        //            ImGui.Text("Our current job is currently not valid.");
        //            return;
        //        }
                
        //        ImGui.AlignTextToFramePadding();
        //        ImGui.Text($"Current job:");
                
        //        // Scaling hardcoded pixel values is important, as otherwise users with HUD scales above or below 100%
        //        // won't be able to see everything.
        //        ImGui.SameLine(120 * ImGuiHelpers.GlobalScale);
                
        //        // Get the icon id from a known offset + the class jobs id
        //        var jobIconId = 62100 + playerState.ClassJob.RowId;
        //        var iconTexture = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(jobIconId)).GetWrapOrEmpty();
        //        ImGui.Image(iconTexture.Handle, new Vector2(28, 28) * ImGuiHelpers.GlobalScale);
                
        //        ImGui.SameLine();
                
        //        // If you want to see the Macro representation of this SeString use `.ToMacroString()`
        //        // More info about SeStrings: https://dalamud.dev/plugin-development/sestring/
        //        ImGui.Text(playerState.ClassJob.Value.Abbreviation.ToString());
                
        //        ImGui.SameLine();
        //        ImGui.Text($" [Level {playerState.Level}]");
                
        //        // Example for querying Lumina, getting the name of our current area.
        //        var territoryId = Plugin.ClientState.TerritoryType;
        //        if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
        //        {
        //            ImGui.Text($"Current location:");
        //            ImGui.SameLine(120 * ImGuiHelpers.GlobalScale);
        //            ImGui.Text(territoryRow.PlaceName.Value.Name.ToString());
        //        }
        //        else
        //        {
        //            ImGui.Text("Invalid territory.");
        //        }
        //    }
        //}
    }
}
