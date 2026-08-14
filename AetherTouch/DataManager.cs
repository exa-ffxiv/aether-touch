using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace AetherTouch
{
    public class DataManager
    {
        private readonly IPluginLog log;

        // Internal Data Folders
        private readonly string configDir;
        private readonly string patternFolder;

        // Data stores, TODO: Maybe split this out or something
        public List<Pattern> Patterns { get; private set; } = [];

        private static readonly JsonSerializerOptions SerializeOptions = new() { WriteIndented = true };

        public DataManager(IDalamudPluginInterface pluginInterface, IPluginLog log)
        {
            this.log = log;
            this.configDir = pluginInterface.GetPluginConfigDirectory();
            this.patternFolder = Path.Combine(configDir, "patterns");
        }

        public void Initialize()
        {
            // Ensure all directories exist
            Directory.CreateDirectory(patternFolder);

            // Load stored data into memeory
            LoadAllPatternsFromFile();
        }

        private void LoadAllPatternsFromFile()
        {
            Patterns.Clear();
            foreach (var file in Directory.EnumerateFiles(patternFolder))
            {
                try
                {
                    if (file.EndsWith(".pattern"))
                    {
                        var p = JsonSerializer.Deserialize<Pattern>(File.ReadAllText(file));
                        if (p != null)
                        {
                            Patterns.Add(p);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Failed to load pattern at \"{file}\". Reason: {ex.Message}");
                }
            }
        }

        public void SavePattern(Pattern p)
        {
            var path = Path.Combine(patternFolder, $"{p.Id}.pattern");
            try
            {
                var json = JsonSerializer.Serialize(p, SerializeOptions);
                File.WriteAllText(path, json);
                int index = Patterns.FindIndex(pattern => pattern.Id == p.Id);
                if (index >= 0)
                {
                    Patterns[index] = p;
                }
                else
                {
                    Patterns.Add(p);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Filed to save pattern to \"{path}\". Reason: {ex.Message}");
            }
        }

        private void SavePatternsToFile()
        {

        }
    }
}
