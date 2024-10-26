using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyMorf
{
    public static class Program
    {
        private static readonly IntPtr KEY_HANDLED = new(1);

        private static IntPtr _hookId = IntPtr.Zero;
        private static readonly Win32.LowLevelKeyboardProc _hookInstance = new(KeyboardHook);
        private static readonly Handler _handler = new();

        public static void Main(string[] args)
        {
            Logger.Level = LogLevel.Debug; 

            Console.CancelKeyPress += (_, _) =>
            {
                Logger.Info("KeyMorf is exiting...");
                Logger.Info("Uninstalling keyboard hook...");
                Win32.UnhookWindowsHookEx(_hookId);

                Logger.Info("Keyboard hook uninstalled.");
                Environment.Exit(0);
            };

            Logger.Info("KeyMorf is starting...");
            Console.Title = "KeyMorf";

            Logger.Info("Getting process and module details...");
            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                Logger.Info("Process and module details retrieved.");
                if (module == null)
                {
                    Logger.Error($"{nameof(module)} was null.");
                }

                if (module!.ModuleName == null)
                {
                    Logger.Error($"{nameof(module.ModuleName)} was null.");
                }

                Logger.Info("Getting module handle...");
                var moduleHandle = Win32.GetModuleHandle(module.ModuleName!);
                if (moduleHandle == IntPtr.Zero)
                {
                    Logger.Error($"Failed to get module handle: {new Win32Exception(Marshal.GetLastWin32Error())}");
                }

                Logger.Info("Module handle retrieved.");

                Logger.Info("Installing keyboard hook...");
                _hookId = Win32.SetWindowsHookEx(Win32.WH_KEYBOARD_LL, _hookInstance, Win32.GetModuleHandle(module.ModuleName!), 0);
                if (_hookId == IntPtr.Zero)
                {
                    Logger.Error($"Failed to install hook: {new Win32Exception(Marshal.GetLastWin32Error())}");
                }

                Logger.Info("Hook installed. KeyMorf is running. Press <C-c> to exit.");

                // This is the message loop. It allows Windows to break into the thread and make the call back.
                // GetMessage retrieves any messages on the current thread's message queue. As low-level
                // hooks send messages instead of queueing them, GetMessage should never actually return.
                var message = new Win32.Message();
                while (Win32.GetMessage(ref message, IntPtr.Zero, 0, 0))
                {
                    Logger.Error($"Recieved a message unexpectedly: {message}. Exiting...");
                }
            }
        }

        private static IntPtr KeyboardHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                // Never seen this happen before...
                Logger.Error($"{nameof(nCode)} was less than zero.");
                return Win32.CallNextHookEx(_hookId, nCode, wParam, lParam);
            }

            if (_handler.Handle(wParam, lParam, Marshal.ReadInt32(lParam)))
            {
                return KEY_HANDLED;
            }

            // For any unhandled keys, let the key be processed as normal.
            return Win32.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }
}
