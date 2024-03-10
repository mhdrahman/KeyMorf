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
    /// <summary>
    /// Contains the startup and run logic for KeyMorf.
    /// </summary>
    public class Program
    {
        private static IntPtr _hookId = IntPtr.Zero;
        private static ConsoleLogger _logger = new ConsoleLogger(LogLevel.Error);
        private static Handler _handler = new Handler(_logger);

        // Constant value which indicates that an event was handled.
        private static IntPtr Handled = new(1);

        /// <summary>
        /// Entry point for the application.
        /// </summary>
        /// <param name="args">Only valid option is --log-level error|warn|info|debug.</param>
        public static void Main(string[] args)
        {
            _hookId = SetKeyboardHook();
            if (_hookId == IntPtr.Zero)
            {
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

        /// <summary>
        /// Install the low-level keyboard hook.
        /// </summary>
        /// <returns>The installed hook id if it was installed successfully.
        /// Returns IntPtr.Zero if an error occured while installing the hook.</returns>
        private static IntPtr SetKeyboardHook()
        {
            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                if (module == null)
                {
                    _logger.Error($"{nameof(module)} was null.");
                    return IntPtr.Zero;
                }

                if (module.ModuleName == null)
                {
                    _logger.Error($"{nameof(module.ModuleName)} was null.");
                    return IntPtr.Zero;
                }

                var moduleHandle = Kernel32.GetModuleHandle(module.ModuleName);
                if (moduleHandle == IntPtr.Zero)
                {
                    _logger.Error($"{new Win32Exception(Marshal.GetLastWin32Error())}");
                    return IntPtr.Zero;
                }

                return User32.SetWindowsHookEx((int)Constants.WH_KEYBOARD_LL, KeyboardHook, Kernel32.GetModuleHandle(module.ModuleName), 0);
            }
        }

        /// <summary>
        /// The keyboard hook to be installed.
        /// </summary>
        /// <param name="nCode">Indicates whether <paramref name="lParam"/> and <paramref name="wParam"/>
        /// contains valid data. A value of 0 indicates valid data.</param>
        /// <param name="wParam">Event type.</param>
        /// <param name="lParam">Virtual key code of the key associated with the event.</param>
        /// <returns></returns>
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