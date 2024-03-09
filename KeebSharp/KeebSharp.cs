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

        // TODO: obviously this doesn't work, just making a single key to a macro but wanna test...
        public static Dictionary<Keys, (Keys Key, bool Shift)[]> Mapping = new()
        {
            [Keys.J] = new[] { (Keys.N, true), (Keys.E, false), (Keys.W, false), (Keys.P, true), (Keys.A, false), (Keys.S, false), (Keys.S, false), (Keys.D1, false), (Keys.D2, false), (Keys.D3, false), (Keys.D1, true) },
        };

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

                if (Mapping.TryGetValue(inputKey, out var mappedKeys))
                {
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
    }
} 