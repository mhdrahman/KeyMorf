using System.Collections.Generic;
using System.Windows.Forms;

namespace KeebSharp.Input
{
    public class Layer
    {
        public int Id { get; set; }

        public Keys ToggleKey { get; set; }

        public bool Active { get; set; };

        public bool ToggleKeyDisabled { get; set; }

        public Dictionary<Keys, Keys[]>? Mappings { get; set; }
    }
}
