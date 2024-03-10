using System.Collections.Generic;
using System.Windows.Forms;

namespace KeebSharp.Input
{
    internal static class KeyMap
    {
        private static Layer LayerOne = new Layer
        {
            Name = "Symbols",
            ToggleKey = Keys.Z,
            ToggleKeyHeld = false,
            ToggleKeyDisabled = false,
            Mappings = new()
            {
                // Brackets yuiop;
                [Keys.Y] = new[] { (Keys.D9, true) },
                [Keys.U] = new[] { (Keys.D0, true) },
                [Keys.I] = new[] { (Keys.OemOpenBrackets, true) },
                [Keys.O] = new[] { (Keys.OemCloseBrackets, true) },
                [Keys.P] = new[] { (Keys.OemOpenBrackets, false) },
                [Keys.OemSemicolon] = new[] { (Keys.OemCloseBrackets, false) },

                // Arrow keys hjkl
                [Keys.H] = new[] { (Keys.Left, false) },
                [Keys.J] = new[] { (Keys.Down, false) },
                [Keys.K] = new[] { (Keys.Up, false) },
                [Keys.L] = new[] { (Keys.Right, false) },

                // Logical operators nm,./
                [Keys.N] = new[] { (Keys.D1, true) },
                [Keys.M] = new[] { (Keys.Oemplus, false) },
                [Keys.Oemcomma] = new[] { (Keys.D7, true) },
                [Keys.OemPeriod] = new[] { (Keys.Oem5, true) },
                [Keys.OemQuestion] = new[] { (Keys.Oem5, false) },

                // Numpad left xcvsdfwer
                [Keys.X] = new[] { (Keys.NumPad1, false) },
                [Keys.C] = new[] { (Keys.NumPad2, false) },
                [Keys.V] = new[] { (Keys.NumPad3, false) },
                [Keys.S] = new[] { (Keys.NumPad4, false) },
                [Keys.D] = new[] { (Keys.NumPad5, false) },
                [Keys.F] = new[] { (Keys.NumPad6, false) },
                [Keys.W] = new[] { (Keys.NumPad7, false) },
                [Keys.E] = new[] { (Keys.NumPad8, false) },
                [Keys.R] = new[] { (Keys.NumPad9, false) },
            },
        };

        private static Layer LayerTwo = new Layer
        {
            Name = "Macros",
            ToggleKey = Keys.Q,
            ToggleKeyHeld = false,
            ToggleKeyDisabled = false,
            Mappings = new()
            {
                [Keys.J] = new[] { (Keys.K, true), (Keys.E, false), (Keys.E, false), (Keys.B, false), (Keys.S, true), (Keys.H, false), (Keys.A, false), (Keys.R, false), (Keys.P, false) },
            },
        };

        public static readonly Dictionary<Keys, Layer> Layers = new Dictionary<Keys, Layer>
        {
            [LayerOne.ToggleKey] = LayerOne,
            [LayerTwo.ToggleKey] = LayerTwo,
        };
    }
}
