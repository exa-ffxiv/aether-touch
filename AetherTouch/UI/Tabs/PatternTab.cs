using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Dalamud.Bindings.ImGui;

namespace AetherTouch.UI.Tabs
{
    public class PatternTab
    {
        private readonly DataManager dataManager;
        private readonly ToyManager toyManager;

        private int selectedPatternIndex = -1;

        public PatternTab(DataManager dataManager, ToyManager toyManager)
        {
            this.dataManager = dataManager;
            this.toyManager = toyManager;
        }
        public void Draw()
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
                if (ImGui.BeginListBox("##PaternList", new Vector2(200f, ImGui.GetWindowHeight() - 85)))
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

        public void DrawPatternDetails()
        {

        }
    }
}
