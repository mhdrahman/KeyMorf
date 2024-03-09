using KeebSharp.Interop;
using KeebSharp.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeebSharp.Handlers
{
    internal class Handler
    {
        private static Keys LayerToggleKey = Keys.A;
        private static bool KeyUpSeen = true;
        private static Stopwatch Timer = new Stopwatch();
        private static bool LayerToggled = false;

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

            if (wParam == Constants.WM_KEYUP)
            {
                _logger.Debug($"Key up: {inputKey}");
                if (inputKey == LayerToggleKey)
                {
                }
            }

            if (wParam == Constants.WM_KEYDOWN)
            {
                _logger.Debug($"Key down: {inputKey}");
                if (inputKey == LayerToggleKey)
                {
                    // Hold back the key for now
                    return true;
                }
            }

            // Let all unhandled keys be processes normally
            return false;
        }

        private static void PressKeys(params (Keys Key, bool Shift)[] keys)
        {
            foreach (var key in keys)
            {
                PressKey(key.Key, key.Shift);
            }
        }

        private static void PressKey(Keys key, bool shift)
        {
            HoldShift(shift);
            User32.keybd_event((byte)key, 0, 0, 0);
            User32.keybd_event((byte)key, 0, 2, 0);
            ReleaseShift(shift);
        }

        private static void HoldShift(bool shift)
        {
            if (!shift)
            {
                return;
            }

            User32.keybd_event((byte)Keys.RShiftKey, 0, 0, 0);
        }

        private static void ReleaseShift(bool shift)
        {
            if (!shift)
            {
                return;
            }

            User32.keybd_event((byte)Keys.RShiftKey, 0, 2, 0);
        }

        private static bool KeyHeld(Keys key)
        {
            return (User32.GetKeyState((int)key) & 0x8000) > 0;
        }
    }
}
