using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KeyMorf
{
    /// <summary>
    /// Contains the keymap, handles the loading of the keymap from config.
    /// </summary>
    internal static class Keymap
    {
        /// <summary>
        /// Path to the keymap configuration file, which contains user-defined key mappings and macros.
        /// </summary>
        private const string KeymapFilePath = "Keymap.json";

        /// <summary>
        /// Mapping of toggle key (<see cref="Keys"/>) to its associated <see cref="Layer"/>.
        /// </summary>
        public static Dictionary<Keys, Layer> Layers { get; } = new();

        /// <summary>
        /// Static constructor which loads the user config file and parses it into <see cref="Layers"/>.
        /// </summary>
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
}
