using KeebSharp.Input;
using KeebSharp.Interop;
using KeebSharp.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeebSharp.Handlers
{
    internal class Handler
    {
        private System.Threading.Timer? Timer;
        private Layer? ActiveLayer;
        private static Layer LayerOne = new Layer
        {
            Name = "Symbols",
            ToggleKey = Keys.Z,
            ToggleKeyHeld = false,
            ToggleKeyDisabled = false,
            Active = false,
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
            Active = false,
            Mappings = new()
            {
                [Keys.J] = new[] { (Keys.K, true), (Keys.E, false), (Keys.E, false), (Keys.B, false), (Keys.S, true), (Keys.H, false), (Keys.A, false), (Keys.R, false), (Keys.P, false) },
            },
        };

        private readonly Dictionary<Keys, Layer> Layers = new Dictionary<Keys, Layer>
        {
            [LayerOne.ToggleKey] = LayerOne,
            [LayerTwo.ToggleKey] = LayerTwo,
        };

        private readonly ConsoleLogger _logger;

        public Handler(ConsoleLogger logger)
        {
            _logger = logger;
        }

        public bool Handle(IntPtr wParam, IntPtr lParam)
        {
            var vkCode = Marshal.ReadInt32(lParam);
            var inputKey = (Keys)vkCode;

            HandleKeyUp(wParam, inputKey);

            // Key down events
            if (wParam == Constants.WM_KEYDOWN)
            {
                if (Layers.TryGetValue(inputKey, out var layer))
                {
                    if (layer.ToggleKeyDisabled)
                    {
                        layer.ToggleKeyDisabled = false;
                        return false;
                    }

                    layer.ToggleKeyHeld = true;
                    void TimerHook(object? state)
                    {
                        if (layer.ToggleKeyHeld)
                        {
                            // If the key has not been released when the timer expires, toggle the layer
                            layer.Active = true;
                            ActiveLayer = layer;
                            _logger.Info($"{layer.Name} layer active.");
                        }
                        else
                        {
                            // Otherwise, let the key through
                            layer.ToggleKeyDisabled = true;
                            Keyboard.Press(layer.ToggleKey, false);
                        }

                        Timer!.Dispose();
                    }

                    if (!layer.Active)
                    {
                        Timer = new System.Threading.Timer(TimerHook, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(25));
                    }

                    // Hold back the key for now, if it doesn't toggle a layer it will be let through 
                    return true;
                }

                if (ActiveLayer != null && ActiveLayer.Active && ActiveLayer.Mappings!.TryGetValue(inputKey, out var mappedKeys))
                {
                    Keyboard.Press(mappedKeys);
                    return true;
                }
            }

            // Let all unhandled keys be processed normally
            return false;
        }

        private void HandleKeyUp(IntPtr wParam, Keys inputKey)
        {
            // Nothing to handle if it's not a key up event
            if (wParam != Constants.WM_KEYUP)
            {
                return;
            }

            // Nothing to handle if we have no layers set to activate with this key
            if (!Layers.TryGetValue(inputKey, out var layer))
            {
                return;
            }

            layer.ToggleKeyHeld = false;
            if (layer.Active)
            {
                layer.Active = false;
                ActiveLayer = null;
                _logger.Info($"{layer.Name} layer inactive.");
            }    
        }
    }
}
