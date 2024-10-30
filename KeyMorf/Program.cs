using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyMorf
{
    /// <summary>
    /// Class containing the main entry point of KeyMorf.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Constant returned to indicate a keyboard input event was handled.
        /// </summary>
        private static readonly IntPtr KEY_HANDLED = new(1);

        /// <summary>
        /// Identifier for the installed hook. Used when calling <see cref="Win32.CallNextHookEx(IntPtr, int, IntPtr, IntPtr)"/> and when uninstalling the hook on application exit..
        /// </summary>
        private static IntPtr _hookId = IntPtr.Zero;

        /// <summary>
        /// Single static instance of the hook to prevent it being killed during GC.
        /// </summary>
        private static readonly Win32.LowLevelKeyboardProc _hookInstance = new(KeyboardHook);

        /// <summary>
        /// The entry point for the KeyMorf application. Initializes the keyboard hook,
        /// sets up a handler to exit the application on Ctrl+C, and starts a message
        /// loop to listen for keyboard events.
        /// </summary>
        public static void Main()
        {
            Logger.Level = LogLevel.Info;

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

            using var process = Process.GetCurrentProcess();
            using var module = process.MainModule;

            if (module == null)
            {
                Logger.Error($"{nameof(module)} was null.");
            }

            if (module!.ModuleName == null)
            {
                Logger.Error($"{nameof(module.ModuleName)} was null.");
            }

            var moduleHandle = Win32.GetModuleHandle(module.ModuleName!);
            if (moduleHandle == IntPtr.Zero)
            {
                Logger.Error($"Failed to get module handle: {new Win32Exception(Marshal.GetLastWin32Error())}");
            }

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

        /// <summary>
        /// Low level keyboard hook delegate. See <see cref="https://learn.microsoft.com/en-us/windows/win32/winmsg/lowlevelkeyboardproc"/> for more information.
        /// </summary>
        /// <param name="nCode">Code used to determine if the message should be processed or not.</param>
        /// <param name="wParam">The identifier of the keyboard message.</param>
        /// <param name="lParam">A pointer to a <see cref="Win32.HookStruct"/>.</param>
        /// <returns>A non-zero value if the message was handled, otherwise the result of calling the <see cref="Win32.CallNextHookEx(IntPtr, int, IntPtr, IntPtr)"/>.</returns>
        private static IntPtr KeyboardHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                // Never seen this happen before...
                Logger.Info($"{nameof(nCode)} was less than zero.");
                return Win32.CallNextHookEx(_hookId, nCode, wParam, lParam);
            }

            if (Handler.Handle(wParam, lParam, Marshal.ReadInt32(lParam)))
            {
                // This prevents the key from being handled by the next hook.
                return KEY_HANDLED;
            }

            // For any unhandled keys, let the key be processed as normal.
            return Win32.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }
}
