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
        private static bool LayerActive = false;
        private static bool KeyUpSeen = true;
        private static System.Threading.Timer? Timer;

        private static Dictionary<Keys, (Keys Key, bool Shift)[]> Layer = new()
        {
            [Keys.Y] = new[] { (Keys.D9, true) },
            [Keys.U] = new[] { (Keys.D0, true) },
            [Keys.I] = new[] { (Keys.OemOpenBrackets, true) },
            [Keys.O] = new[] { (Keys.OemCloseBrackets, true) },
            [Keys.P] = new[] { (Keys.OemOpenBrackets, false) },
            [Keys.OemSemicolon] = new[] { (Keys.OemCloseBrackets, false) },
            [Keys.H] = new[] { (Keys.Left, false) },
            [Keys.J] = new[] { (Keys.Down, false) },
            [Keys.K] = new[] { (Keys.Up, false) },
            [Keys.L] = new[] { (Keys.Right, false) },
            [Keys.N] = new[] { (Keys.D1, true) },
            [Keys.M] = new[] { (Keys.Oemplus, false) },
            [Keys.Oemcomma] = new[] { (Keys.D7, true) },
            [Keys.OemPeriod] = new[] { (Keys.Oem5, true) },
            [Keys.OemQuestion] = new[] { (Keys.Oem5, false) },
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
                    KeyUpSeen = true;
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
                _logger.Debug(inputKey.ToString());
                if (inputKey == LayerToggleKey)
                {
                    KeyUpSeen = false;
                    void TimerHook(object? state)
                    {
                        // If the key has not been released when the timer expires, toggle the layer
                        if (!KeyUpSeen)
                        {
                            _logger.Info("Layer active.");
                            LayerActive = true;
                        }

                        Timer!.Dispose();
                    }

                    if (!LayerActive)
                    {
                        Timer = new System.Threading.Timer(TimerHook, null, TimeSpan.FromMilliseconds(500), TimeSpan.Zero);
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

            // Let all unhandled keys be processes normally
            return false;
        }
    }
}
