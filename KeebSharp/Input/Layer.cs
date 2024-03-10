using System.Collections.Generic;
using System.Windows.Forms;

namespace KeebSharp.Input
{
    public class Layer
    {
        public string? Name { get; set; }

        public Keys ToggleKey { get; set; }

        public bool ToggleKeyHeld { get; set; }

        public bool ToggleKeyDisabled { get; set; }

        public bool Active { get; set; }

        public Dictionary<Keys, (Keys Key, bool Shift)[]>? Mappings { get; set; }
    }
}
