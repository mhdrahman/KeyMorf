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
        private static Keys LayerToggleKey = Keys.Z;
        private static bool LayerToggleKeyHeld = true;
        private static bool LayerToggleKeyDisabled = false;
        private static bool LayerActive = false;
        private static System.Threading.Timer? Timer;

        private static Dictionary<Keys, (Keys Key, bool Shift)[]> Layer = new()
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
        };

        private static Layer LayerOne = new Layer
        {
            Id = 1,
            ToggleKey = Keys.Z,
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
            Id = 2,
            ToggleKey = Keys.Q,
            Active = false,
            Mappings = new()
            {
                [Keys.J] = new[] { (Keys.H, true), (Keys.E, false), (Keys.L, false), (Keys.L, false), (Keys.O, false), (Keys.Oemcomma, false), (Keys.Space, false) },
                [Keys.K] = new[] { (Keys.W, true), (Keys.O, false), (Keys.R, false), (Keys.L, false), (Keys.D, false) },
                [Keys.K] = new[] { (Keys.D1, true) },
            },
        };

        private static Dictionary<Keys, Layer> Layers = new Dictionary<Keys, Layer>
        {
            [LayerOne.ToggleKey] = LayerOne,
            [LayerTwo.ToggleKey] = LayerTwo,
        };

        private readonly ConsoleLogger _logger;

        public Handler(ConsoleLogger logger)
        {
            _logger = logger;
        }

        public bool Handle(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                return false;
            }

            var vkCode = Marshal.ReadInt32(lParam);
            var inputKey = (Keys)vkCode;

            // Key up events
            if (wParam == Constants.WM_KEYUP)
            {
                if (inputKey == LayerToggleKey)
                {
                    LayerToggleKeyHeld = false;
                    if (LayerActive)
                    {
                        LayerActive = false;
                        _logger.Info("Layer inactive.");
                    }    
                }
            }

            // Key down events
            if (wParam == Constants.WM_KEYDOWN)
            {
                if (inputKey == LayerToggleKey)
                {
                    if (LayerToggleKeyDisabled)
                    {
                        LayerToggleKeyDisabled = false;
                        return false;
                    }

                    LayerToggleKeyHeld = true;
                    void TimerHook(object? state)
                    {
                        if (LayerToggleKeyHeld)
                        {
                            // If the key has not been released when the timer expires, toggle the layer
                            LayerActive = true;
                            _logger.Info("Layer active.");
                        }
                        else
                        {
                            // Otherwise, let the key through
                            LayerToggleKeyDisabled = true;
                            Keyboard.Press(LayerToggleKey, false);
                        }

                        Timer!.Dispose();
                    }

                    if (!LayerActive)
                    {
                        // TODO: maybe wrap up the timer, so don't have to look at ugly full namespace
                        Timer = new System.Threading.Timer(TimerHook, null, TimeSpan.FromMilliseconds(100), TimeSpan.Zero);
                    }

                    // Hold back the key for now
                    return true;
                }

                if (LayerActive && Layer.TryGetValue(inputKey, out var mappedKeys))
                {
                    Keyboard.Press(mappedKeys);
                    return true;
                }
            }

            // Let all unhandled keys be processed normally
            return false;
        }
    }
}
