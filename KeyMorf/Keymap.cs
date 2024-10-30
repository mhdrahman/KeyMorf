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

                foreach (var kvp in config!)
                {
                    var layerName = kvp.Key;
                    var userKeymap = kvp.Value;

                    // TODO: Not sure how I feel about this nasty LINQ statement, might be better to just use those User-x classes here instead of mapping...
                    var layer = new Layer(
                        layerName,
                        userKeymap.ToggleKey,
                        userKeymap.ToggleTimeMs,
                        userKeymap.Mappings.ToDictionary(userMapping => userMapping.From, userMapping => (userMapping.To, userMapping.Mods)),
                        userKeymap.Macros.ToDictionary(userMacro => userMacro.ToggleKey, userMacro => userMacro.Macro.Select(userMacroKey => (userMacroKey.Key, userMacroKey.Mods)).ToArray()));

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

        public Dictionary<Keys, (Keys Key, Keys[]? Mods)[]> Macros { get; }

        public Layer(string name, Keys toggleKey, int toggleTimeMs, Dictionary<Keys, (Keys Key, Keys[]? Mods)> mappings, Dictionary<Keys, (Keys Key, Keys[]? Mods)[]> macros)
        {
            Name = name;
            ToggleKey = toggleKey;
            ToggleTimeMs = toggleTimeMs;
            Mappings = mappings;
            Macros = macros;
        }
    }

    public class UserKeymap
    {
        public Keys ToggleKey { get; }

        public int ToggleTimeMs { get; }

        public UserMapping[] Mappings { get; }

        public UserMacro[] Macros { get; }

        public UserKeymap(Keys toggleKey, int toggleTimeMs, UserMapping[] mappings, UserMacro[] macros)
        {
            ToggleKey = toggleKey;
            ToggleTimeMs = toggleTimeMs;
            Mappings = mappings;
            Macros = macros;
        }

        public class UserMapping
        {
            public Keys From { get; }

            public Keys To { get; }

            public Keys[]? Mods { get; }

            public UserMapping(Keys from, Keys to, Keys[]? mods)
            {
                From = from;
                To = to;
                Mods = mods;
            }
        }

        public class UserMacro
        {
            public Keys ToggleKey { get; }

            public UserMacroKey[] Macro { get; }

            public UserMacro(Keys toggleKey, UserMacroKey[] macro)
            {
                ToggleKey = toggleKey;
                Macro = macro;
            }

            public class UserMacroKey
            {
                public Keys Key { get; }

                public Keys[]? Mods { get; }

                public UserMacroKey(Keys key, Keys[]? mods)
                {
                    Key = key;
                    Mods = mods;
                }
            }
        }
    }
}
