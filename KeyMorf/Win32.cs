using System;
using System.Runtime.InteropServices;

namespace KeyMorf
{
    /// <summary>
    /// Class containing Win32 API interop stuff. See <see cref="https://learn.microsoft.com/en-us/windows/win32/api/"/> for more info on each of these.
    /// </summary>
    internal static class Win32
    {
        #region Constants

        public const int WH_KEYBOARD_LL = 13;
        public const int KEYEVENTF_KEYDOWN = 0;
        public const int KEYEVENTF_KEYUP = 2;
        public const int LLKHF_INJECTED = 16;

        public static readonly IntPtr WM_KEYDOWN = new(256);
        public static readonly IntPtr WM_KEYUP = new(257);

        #endregion Constants

        #region Methods

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool GetMessage(ref Message message, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        #endregion Methods

        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        public struct HookStruct { public uint vkCode; public uint scanCode; public uint flags; public uint time; public IntPtr dwExtraInfo; }

        [StructLayout(LayoutKind.Sequential)]
        public struct Message { public IntPtr hwnd; public uint message; public UIntPtr wParam; public IntPtr lParam; public uint time; public Point pt; }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point { public long x; public long y; }

        #endregion Structs
    }
}
