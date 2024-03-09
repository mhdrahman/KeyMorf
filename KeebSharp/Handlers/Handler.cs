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
        private static Stopwatch _toggleTimer = new Stopwatch();
        private static bool _toggled = false;

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
        private readonly IntPtr _hookId;

        public Handler(ConsoleLogger logger, IntPtr hookId)
        {
            _logger = logger;
            _hookId = hookId;
        }

        public bool Handle(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == Hooks.WM_KEYDOWN)
            {
                var vkCode = Marshal.ReadInt32(lParam);
                var inputKey = (Keys)vkCode;

                if (inputKey == LayerToggleKey)
                {
                    if (!_toggleTimer.IsRunning)
                    {
                        _toggleTimer.Start();
                        return false;
                    }

                    if (_toggleTimer.ElapsedMilliseconds < 500)
                    {
                        _toggled = !_toggled;
                        _logger.Info(_toggled ? "Layer toggled." : "Layer disabled.");
                        _toggleTimer.Stop();
                        _toggleTimer.Reset();
                        return true;
                    }
                    else
                    {
                        _toggleTimer.Stop();
                        _toggleTimer.Reset();
                    }
                }

                if (_toggled && Layer.TryGetValue(inputKey, out var mappedKeys))
                {
                    if (IsKeyPressed(Keys.LShiftKey))
                    {
                        _logger.Info("Left shift pressed");
                    }

                    _logger.Info($"Mapped key pressed {inputKey}.");
                    PressKeys(mappedKeys);
                    return true;
                }

                // For any unmapped keys, let the key be processed as normal
                return false;
            }


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

        private static bool IsKeyPressed(Keys key)
        {
            return (User32.GetKeyState((int)Keys.LShiftKey) & 0x8000) > 0;
        }
    }
}
