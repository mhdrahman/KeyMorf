using KeebSharp.Interop;
using KeebSharp.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeebSharp
{
    public static class KeebSharp
    {
        private static ConsoleLogger _logger = new ConsoleLogger();
        private static IntPtr _hookId = IntPtr.Zero;

        private static IntPtr Handled = new(1);

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

        private static Keys LayerToggleKey = Keys.A;
        private static Stopwatch _toggleTimer = new Stopwatch();
        private static bool _toggled = false;

        public static void Start()
        {
            _hookId = SetKeyboardHook();
            if (_hookId == IntPtr.Zero)
            {
                _logger.Win32();
                Environment.Exit(-1);
            }

            Console.CancelKeyPress += (_, _) =>
            {
                _logger.Info("KeebSharp is exiting. Please wait...");

                Application.Exit();
                User32.UnhookWindowsHookEx(_hookId);
                Environment.Exit(0);
            };

            _logger.Info("KeebSharp is running. Press <C-c> to exit.");
            Application.Run();
        }

        private static IntPtr SetKeyboardHook()
        {
            using (var currentProcess = Process.GetCurrentProcess())
            using (var currentModule = currentProcess.MainModule)
            {
                if (currentModule == null)
                {
                    _logger.Error($"ERROR: {nameof(currentModule)} was null.");
                    return IntPtr.Zero;
                }

                if (currentModule.ModuleName == null)
                {
                    _logger.Error($"ERROR: {nameof(currentModule.ModuleName)} was null.");
                    return IntPtr.Zero;
                }

                var moduleHandle = Kernel32.GetModuleHandle(currentModule.ModuleName);
                if (moduleHandle == IntPtr.Zero)
                {
                    _logger.Error($"ERROR: {new Win32Exception(Marshal.GetLastWin32Error())}");
                    return IntPtr.Zero;
                }

                return User32.SetWindowsHookEx((int)Hooks.WH_KEYBOARD_LL, KeyboardHookCallback, Kernel32.GetModuleHandle(currentModule.ModuleName), 0);
            }
        }

        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
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
                        return User32.CallNextHookEx(_hookId, nCode, Hooks.WM_KEYDOWN, (IntPtr)vkCode);
                    }

                    if (_toggleTimer.ElapsedMilliseconds < 500)
                    {
                        _toggled = !_toggled;
                        _logger.Info(_toggled ? "Layer toggled." : "Layer disabled.");
                        _toggleTimer.Stop();
                        _toggleTimer.Reset();
                        return Handled;
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
                    return Handled;
                }

                // For any unmapped keys, let the key be processed as normal
                return User32.CallNextHookEx(_hookId, nCode, Hooks.WM_KEYDOWN, (IntPtr)vkCode);
            }

            return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
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