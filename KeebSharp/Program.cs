using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace KeebSharp
{
    public class Program
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private static IntPtr _hookID = IntPtr.Zero;
        private static int _installingThreadId = 0;
        private const string DataDirecory = "C:/ProgramData/KeebSharp";

        public static void Main(string[] args)
        {
            Directory.CreateDirectory(DataDirecory);

            // Set the hook
            _hookID = SetHook(HookCallback);
            if (_hookID == IntPtr.Zero)
            {
                Console.WriteLine($"ERROR: {new Win32Exception(Marshal.GetLastWin32Error())}");
                Environment.Exit(-1);
            }

            Console.CancelKeyPress += (_, _) =>
            {
                Application.Exit();
                UnhookWindowsHookEx(_hookID);
                Environment.Exit(0);
            };

            Console.WriteLine("KeebSharp is running. Press <C-c> to exit.");
            Application.Run();
        }

        private static IntPtr SetHook(HookProc proc)
        {
            _installingThreadId = Thread.CurrentThread.ManagedThreadId;

            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                if (curModule == null)
                {
                    Console.WriteLine($"ERROR: {nameof(curModule)} was null.");
                    return IntPtr.Zero;
                }

                if (curModule.ModuleName == null)
                {
                    Console.WriteLine($"ERROR: {nameof(curModule.ModuleName)} was null.");
                    return IntPtr.Zero;
                }

                var moduleHandle = GetModuleHandle(curModule.ModuleName);
                if (moduleHandle == IntPtr.Zero)
                {
                    Console.WriteLine($"ERROR: {new Win32Exception(Marshal.GetLastWin32Error())}");
                    return IntPtr.Zero;
                }

                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (_installingThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Console.WriteLine("ERROR: Hook callback running on a different thread than the one that installed the hook.");
                return IntPtr.Zero;
            }

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                var vkCode = Marshal.ReadInt32(lParam);
                Console.WriteLine($"INFO: Virtual key code - '{vkCode}'.");

                return CallNextHookEx(_hookID, nCode, (IntPtr)WM_KEYDOWN, (IntPtr)vkCode);
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
