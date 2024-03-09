using KeebSharp.Interop;
using KeebSharp.Logging;
using System;
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

        public static void Start()
        {
            _hookId = SetKeyboardHook();
            if (_hookId == IntPtr.Zero)
            {
                // TODO: Method for logging Win32 error
                _logger.Error($"{new Win32Exception(Marshal.GetLastWin32Error())}");
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

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == Hooks.WM_KEYDOWN)
            {
                var vkCode = Marshal.ReadInt32(lParam);
                _logger.Info($"INFO: Key pressed. Virtual key code - '{vkCode}'.");

                // Remap j to a
                if (vkCode == 74)
                {
                    keybd_event(0x41, 0, 0, 0);
                    keybd_event(0x41, 0, 2, 0);

                    // Return 1 to say it's been handled
                    return (IntPtr)1;
                }

                return User32.CallNextHookEx(_hookId, nCode, Hooks.WM_KEYDOWN, (IntPtr)vkCode);
            }

            return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }
}
