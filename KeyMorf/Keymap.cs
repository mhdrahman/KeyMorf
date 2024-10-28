using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KeyMorf
{
    public static class Keymap
    {
        private const string KeymapFilePath = "Keymap.json";

        public static Dictionary<Keys, Layer> Layers { get; } = new();

#pragma warning disable S3963 // "static" fields should be initialized inline - inlining the initialisation of Layers would be weird...
        static Keymap()
#pragma warning restore S3963 // "static" fields should be initialized inline
        {
            try
            {
                var configJson = File.ReadAllText(KeymapFilePath);
                var config = JsonConvert.DeserializeObject<Dictionary<string, UserKeymap>>(configJson);

                if (config is null)
                {
                    Logger.Error($"Deserialising '{KeymapFilePath}' returned a null value");
                }

                // Convert the config (designed to be easy to read into a structure that's easy to interact with).
                foreach (var kvp in config!)
                {
                    var layerName = kvp.Key;
                    var userKeymap = kvp.Value;

                    var layer = new Layer(
                        layerName,
                        userKeymap.ToggleKey,
                        userKeymap.ToggleTimeMs,
                        userKeymap.Mappings.ToDictionary(userMapping => userMapping.From, userMapping => (userMapping.To, userMapping.Mods)));

                    Layers[layer.ToggleKey] = layer;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to initialise the keymap. Error: {ex}");
            }
        }
    }

    public class Layer
    {
        public string Name { get; set; }

        public Keys ToggleKey { get; }

        public int ToggleTimeMs { get; }

        public bool Toggled { get; set; }

        public bool TogglePending { get; set; }

        public Dictionary<Keys, (Keys Key, Keys[]? Mods)> Mappings { get; }

        public Layer(string name, Keys toggleKey, int toggleTimeMs, Dictionary<Keys, (Keys Key, Keys[]? Mods)> mappings)
        {
            Name = name;
            ToggleKey = toggleKey;
            ToggleTimeMs = toggleTimeMs;
            Mappings = mappings;
        }
    }
}
