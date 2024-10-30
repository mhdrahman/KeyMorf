namespace KeyMorf
{
    /// <summary>
    /// Typed representation of the user defined keymap JSON.
    /// </summary>
    internal class UserKeymap
    {
        /// <summary>
        /// Key which toggles this layer.
        /// </summary>
        public Keys ToggleKey { get; }

        /// <summary>
        /// How long (in milliseconds) the <see cref="ToggleKey"/> should be held before the layer is toggled.
        /// </summary>
        public int ToggleTimeMs { get; }

        /// <summary>
        /// The layer's key remappings. Array of <see cref="UserMapping"/>.
        /// </summary>
        public UserMapping[] Mappings { get; }

        /// <summary>
        /// The layer's macros. Array of <see cref="UserMacro"/>.
        /// </summary>
        public UserMacro[] Macros { get; }

        /// <summary>
        /// Initialise an instance of <see cref="UserKeymap"/>.
        /// </summary>
        /// <param name="toggleKey"><see cref="ToggleKey"/></param>
        /// <param name="toggleTimeMs"><see cref="ToggleTimeMs"/></param>
        /// <param name="mappings"><see cref="Mappings"/></param>
        /// <param name="macros"><see cref="Macros"/></param>
        public UserKeymap(Keys toggleKey, int toggleTimeMs, UserMapping[] mappings, UserMacro[] macros)
        {
            ToggleKey = toggleKey;
            ToggleTimeMs = toggleTimeMs;
            Mappings = mappings;
            Macros = macros;
        }

        /// <summary>
        /// Typed representation of a user defined key remapping JSON.
        /// </summary>
        public class UserMapping
        {
            /// <summary>
            /// Key (<see cref="Keys"/> to be mapped.
            /// </summary>
            public Keys From { get; }

            /// <summary>
            /// Key (<see cref="Keys"/> that <see cref="From"/> should be mapped to.
            /// </summary>
            public Keys To { get; }

            /// <summary>
            /// Any modifier <see cref="Keys"/> to also be pressed with the <see cref="To"/> key.
            /// </summary>
            public Keys[]? Mods { get; }

            /// <summary>
            /// Initialise an instance of <see cref="UserMapping"/>.
            /// </summary>
            /// <param name="from"><see cref="From"/></param>
            /// <param name="to"><see cref="To"/></param>
            /// <param name="mods"><see cref="Mods"/></param>
            public UserMapping(Keys from, Keys to, Keys[]? mods)
            {
                From = from;
                To = to;
                Mods = mods;
            }
        }

        /// <summary>
        /// Typed representation of a user defined macro JSON.
        /// </summary>
        public class UserMacro
        {
            /// <summary>
            /// Key (<see cref="Keys"/>) to toggle to macro.
            /// </summary>
            public Keys ToggleKey { get; }

            /// <summary>
            /// Macro to be run when <see cref="ToggleKey"/> is pressed. Array of <see cref="UserMacroKey"/>.
            /// </summary>
            public UserMacroKey[] Macro { get; }

            /// <summary>
            /// Intialise an instance of <see cref="UserMacro"/>.
            /// </summary>
            /// <param name="toggleKey"><see cref="ToggleKey"/></param>
            /// <param name="macro"><see cref="Macro"/></param>
            public UserMacro(Keys toggleKey, UserMacroKey[] macro)
            {
                ToggleKey = toggleKey;
                Macro = macro;
            }

            /// <summary>
            /// Typed representation of a user defined macro key JSON.
            /// </summary>
            public class UserMacroKey
            {
                /// <summary>
                /// A single key (<see cref="Keys"/>) in the macro.
                /// </summary>
                public Keys Key { get; }

                /// <summary>
                /// Any modifier <see cref="Keys"/> to also be pressed with the <see cref="Key"/>.
                /// </summary>
                public Keys[]? Mods { get; }

                /// <summary>
                /// Initialise an instance of <see cref="UserMacroKey"/>
                /// </summary>
                /// <param name="key"><see cref="Key"/></param>
                /// <param name="mods"><see cref="Mods"/></param>
                public UserMacroKey(Keys key, Keys[]? mods)
                {
                    Key = key;
                    Mods = mods;
                }
            }
        }
    }
}
