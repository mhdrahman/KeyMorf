using System.Collections.Generic;

namespace KeyMorf
{
    /// <summary>
    /// Class representing a toggleable layer which contains the layer's <see cref="Mappings"/>, <see cref="Macros"/> and state.
    /// </summary>
    internal class Layer
    {
        /// <summary>
        /// Friendly name of the layer.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Key which toggles this layer.
        /// </summary>
        public Keys ToggleKey { get; }

        /// <summary>
        /// How long (in milliseconds) the <see cref="ToggleKey"/> should be held before the layer is toggled.
        /// </summary>
        public int ToggleTimeMs { get; }

        /// <summary>
        /// Whether or not the layer
        /// </summary>
        public bool Toggled { get; set; }

        /// <summary>
        /// Whether or not the layer has had its <see cref="ToggleKey"/> pressed and is a pending a toggle.
        /// </summary>
        public bool TogglePending { get; set; }

        /// <summary>
        /// The layer's key remappings.
        /// Mapping of the remapped key (<see cref="Keys"/>) to a tuple of the remapped key (<see cref="Keys"/>) and any modifier keys.
        /// </summary>
        public Dictionary<Keys, (Keys Key, Keys[]? Mods)> Mappings { get; }

        /// <summary>
        /// The layer's macros.
        /// Mapping of the macro toggle key (<see cref="Keys"/>) to an array of tuple of the macro <see cref="Keys"/> and any modifier keys.
        /// </summary>
        public Dictionary<Keys, (Keys Key, Keys[]? Mods)[]> Macros { get; }

        /// <summary>
        /// Intitialis an instance of <see cref="Layer"/>.
        /// </summary>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="toggleKey"><see cref="ToggleKey"/></param>
        /// <param name="toggleTimeMs"><see cref="ToggleTimeMs"/></param>
        /// <param name="mappings"><see cref="Mappings"/></param>
        /// <param name="macros"><see cref="Macros"/></param>
        public Layer(
            string name,
            Keys toggleKey,
            int toggleTimeMs,
            Dictionary<Keys, (Keys Key, Keys[]? Mods)> mappings,
            Dictionary<Keys, (Keys Key, Keys[]? Mods)[]> macros)
        {
            Name = name;
            ToggleKey = toggleKey;
            ToggleTimeMs = toggleTimeMs;
            Mappings = mappings;
            Macros = macros;
        }
    }
}
