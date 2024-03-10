using KeebSharp.Handlers;
using KeebSharp.Interop;
using KeebSharp.Logging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeebSharp
{
    public class Program
    {
        private static IntPtr _hookId = IntPtr.Zero;
        private static ConsoleLogger _logger = new ConsoleLogger();
        private static Handler _handler = new Handler(_logger);

        private static IntPtr Handled = new(1);

        public static void Main(string[] args)
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
            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                if (module == null)
                {
                    _logger.Error($"ERROR: {nameof(module)} was null.");
                    return IntPtr.Zero;
                }

                if (module.ModuleName == null)
                {
                    _logger.Error($"ERROR: {nameof(module.ModuleName)} was null.");
                    return IntPtr.Zero;
                }

                var moduleHandle = Kernel32.GetModuleHandle(module.ModuleName);
                if (moduleHandle == IntPtr.Zero)
                {
                    _logger.Error($"ERROR: {new Win32Exception(Marshal.GetLastWin32Error())}");
                    return IntPtr.Zero;
                }

                return User32.SetWindowsHookEx((int)Constants.WH_KEYBOARD_LL, KeyboardHook, Kernel32.GetModuleHandle(module.ModuleName), 0);
            }
        }

        private static IntPtr KeyboardHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                // Never seen this happen before...
                _logger.Warn($"{nameof(nCode)} was less than zero");
                return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
            }

            // _handler won't be null here because it's set in the Start function
            var handled = _handler!.Handle(wParam, lParam);
            if (handled)
            {
                return Handled;
            }    

            // For any unhandled keys, let the key be processed as normal
            return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }
} 