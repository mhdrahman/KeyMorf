using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace KeyMorf
{
    public static class Program
    {
        private const int WH_KEYBOARD_LL = 13;

        private static IntPtr _hookId = IntPtr.Zero;
        private static readonly Win32.LowLevelKeyboardProc _hookInstance = new(KeyboardHook);

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += (_, _) =>
            {
                Logger.Info("KeyMorf is exiting. Please wait...");
                Logger.Info("Uninstalling keyboard hook...");
                Win32.UnhookWindowsHookEx(_hookId);

                Logger.Info("Keyboard hook uninstalled.");
                Environment.Exit(0);
            };

            Logger.Info("KeyMorf is starting. Please wait...");
            Console.Title = "KeyMorf";

            Logger.Info("Getting process and module details...");
            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                Logger.Info("Process and module details retrieved.");
                if (module == null)
                {
                    Logger.Error($"{nameof(module)} was null.");
                    Environment.Exit(1);
                }

                if (module.ModuleName == null)
                {
                    Logger.Error($"{nameof(module.ModuleName)} was null.");
                    Environment.Exit(1);
                }

                Logger.Info("Getting module handle...");
                var moduleHandle = Win32.GetModuleHandle(module.ModuleName);
                if (moduleHandle == IntPtr.Zero)
                {
                    Logger.Error($"{new Win32Exception(Marshal.GetLastWin32Error())}");
                    Environment.Exit(1);
                }

                Logger.Info("Module handle retrieved.");

                Logger.Info("Installing keyboard hook...");
                _hookId = Win32.SetWindowsHookEx(WH_KEYBOARD_LL, _hookInstance, Win32.GetModuleHandle(module.ModuleName), 0);
                if (_hookId == IntPtr.Zero)
                {
                    Logger.Error($"Failed to install hook. Exiting...");
                    Environment.Exit(1);
                }

                Logger.Info("Hook installed. KeyMorf is running. Press <C-c> to exit.");

                // This is the message loop. It allows Windows to break into the thread and make the call back.
                // GetMessage retrieves any messages on the current thread's message queue. As low-level
                // hooks send messages instead of queueing them, GetMessage should never actually return.
                var message = new Win32.Message();
                while (Win32.GetMessage(ref message, IntPtr.Zero, 0, 0))
                {
                    Logger.Error($"Recieved a message unexpectedly. Exiting...");
                    Environment.Exit(1);
                }
            }
        }

        private static IntPtr KeyboardHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                // Never seen this happen before...
                Logger.Warn($"{nameof(nCode)} was less than zero");
                return Win32.CallNextHookEx(_hookId, nCode, wParam, lParam);
            }

            var keyCode = Marshal.ReadInt32(lParam);

            string keyUpOrDown;
            if (wParam == new IntPtr(0x0101))
            {
                keyUpOrDown = "KEY UP";
            }
            else if (wParam == new IntPtr(0x0100))
            {
                keyUpOrDown = "KEY DOWN";
            }
            else
            {
                keyUpOrDown = "UNKNOWN";
            }

            Logger.Debug($"{keyUpOrDown}: {Keyboard.GetKey(keyCode)}");

            // For any unhandled keys, let the key be processed as normal
            return Win32.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }

    public class Key
    {
        public int KeyCode { get; }

        public string KeyChar { get; }

        public string ShiftedKeyChar { get; }

        public Key(int keyCode, string keyChar, string shiftedKeyChar)
        {
            KeyCode = keyCode;
            KeyChar = keyChar;
            ShiftedKeyChar = shiftedKeyChar;
        }

        public override string ToString()
            => $"Key code: {KeyCode}, Key char: {KeyChar}, Shifted key char: {ShiftedKeyChar}";
    }

    public static class Keyboard
    {
        // Numbers
        public const int Zero = 48;
        public const int One = 49;
        public const int Two = 50;
        public const int Three = 51;
        public const int Four = 52;
        public const int Five = 53;
        public const int Six = 54;
        public const int Seven = 55;
        public const int Eight = 56;
        public const int Nine = 57;

        // Letters
        public const int A = 65;
        public const int B = 66;
        public const int C = 67;
        public const int D = 68;
        public const int E = 69;
        public const int F = 70;
        public const int G = 71;
        public const int H = 72;
        public const int I = 73;
        public const int J = 74;
        public const int K = 75;
        public const int L = 76;
        public const int M = 77;
        public const int N = 78;
        public const int O = 79;
        public const int P = 80;
        public const int Q = 81;
        public const int R = 82;
        public const int S = 83;
        public const int T = 84;
        public const int U = 85;
        public const int V = 86;
        public const int W = 87;
        public const int X = 88;
        public const int Y = 89;
        public const int Z = 90;

        // Symbols
        public const int Backtick = 223;
        public const int Minus = 189;
        public const int Equal = 187;
        public const int LSquareBracket = 219;
        public const int RSquareBracket = 221;
        public const int Semicolon = 186;
        public const int Apostrophe = 192;
        public const int Hash = 222;
        public const int Backslash = 220;
        public const int Comma = 188;
        public const int Fullstop = 190;
        public const int ForwardSlash = 191;

        // Movement and editing
        public const int Escape = 27;
        public const int Delete = 46;
        public const int Backspace = 8;
        public const int PageUp = 33;
        public const int Tab = 9;
        public const int Enter = 13;
        public const int PageDown = 34;
        public const int Caps = 20;
        public const int Home = 36;
        public const int LShift = 160;
        public const int RShift = 161;
        public const int Up = 38;
        public const int End = 35;
        public const int LControl = 162;
        public const int Win = 91;
        public const int LAlt = 164;
        public const int Space = 32;
        public const int RAlt = 165;
        public const int RControl = 163;
        public const int Left = 37;
        public const int Down = 40;
        public const int Right = 39;

        private static Dictionary<int, Key> KeyCodes = new()
        {
            // Numbers
            [Zero] = new(Zero, "0", ")"),
            [One] = new(One, "1", "!"),
            [Two] = new(Two, "2", "\""),
            [Three] = new(Three, "3", "£"),
            [Four] = new(Four, "4", "$"),
            [Five] = new(Five, "5", "%"),
            [Six] = new(Six, "6", "^"),
            [Seven] = new(Seven, "7", "&"),
            [Eight] = new(Eight, "8", "*"),
            [Nine] = new(Nine, "9", "("),

            // Letters
            [A] = new(A, "a", "A"),
            [B] = new(B, "b", "B"),
            [C] = new(C, "c", "C"),
            [D] = new(D, "d", "D"),
            [E] = new(E, "e", "E"),
            [F] = new(F, "f", "F"),
            [G] = new(G, "g", "G"),
            [H] = new(H, "h", "H"),
            [I] = new(I, "i", "I"),
            [J] = new(J, "j", "J"),
            [K] = new(K, "k", "K"),
            [L] = new(L, "l", "L"),
            [M] = new(M, "m", "M"),
            [N] = new(N, "n", "N"),
            [O] = new(O, "o", "O"),
            [P] = new(P, "p", "P"),
            [Q] = new(Q, "q", "Q"),
            [R] = new(R, "r", "R"),
            [S] = new(S, "s", "S"),
            [T] = new(T, "t", "T"),
            [U] = new(U, "u", "U"),
            [V] = new(V, "v", "V"),
            [W] = new(W, "w", "W"),
            [X] = new(X, "x", "X"),
            [Y] = new(Y, "y", "Y"),
            [Z] = new(Z, "z", "Z"),

            // Symbols
            [Backtick] = new(Backtick, "`", "¬"),
            [Minus] = new(Minus, "-", "_"),
            [Equal] = new(Equal, "=", "+"),
            [LSquareBracket] = new(LSquareBracket, "[", "{"),
            [RSquareBracket] = new(RSquareBracket, "]", "}"),
            [Semicolon] = new(Semicolon, ";", ":"),
            [Apostrophe] = new(Apostrophe, "'", "@"),
            [Hash] = new(Hash, "#", "~"),
            [Backslash] = new(Backslash, "\\", "|"),
            [Comma] = new(Comma, ",", "<"),
            [Fullstop] = new(Fullstop, ".", ">"),
            [ForwardSlash] = new(ForwardSlash, "/", "?"),

            // Movement & editing
            [Escape] = new(Escape, "ESCAPE", "ESCAPE"),
            [Delete] = new(Delete, "DELETE", "DELETE"),
            [Backspace] = new(Backspace, "BACKSPACE", "BACKSPACE"),
            [PageUp] = new(PageUp, "PAGEUP", "PAGEUP"),
            [Tab] = new(Tab, "TAB", "TAB"),
            [Enter] = new(Enter, "ENTER", "ENTER"),
            [PageDown] = new(PageDown, "PAGEDOWN", "PAGEDOWN"),
            [Caps] = new(Caps, "CAPS", "CAPS"),
            [Home] = new(Home, "HOME", "HOME"),
            [LShift] = new(LShift, "LEFT SHIFT", "LEFT SHIFT"),
            [RShift] = new(RShift, "RIGHT SHIFT", "RIGHT SHIFT"),
            [Up] = new(Up, "UP ARROW", "UP ARROW"),
            [End] = new(End, "END", "END"),
            [LControl] = new(LControl, "LEFT CONTROL", "LEFT CONTROL"),
            [Win] = new(Win, "WINDOWS", "WINDOWS"),
            [LAlt] = new(LAlt, "LEFT ALT", "LEFT ALT"),
            [Space] = new(Space, "SPACE", "SPACE"),
            [RAlt] = new(RAlt, "RIGHT ALT", "RIGHT ALT"),
            [RControl] = new(RControl, "RIGHT CONTROL", "RIGHT CONTROL"),
            [Left] = new(Left, "LEFT ARROW", "LEFT ARROW"),
            [Down] = new(Down, "DOWN ARROW", "DOWN ARROW"),
            [Right] = new(Right, "RIGHT ARROW", "RIGHT ARROW"),
        };

        public static string? GetKey(int keyCode)
        {
            if (!KeyCodes.TryGetValue(keyCode, out var key))
            {
                return null;
            }

            if (IsKeyDown(LShift) || IsKeyDown(RShift))
            {
                return key.ShiftedKeyChar;
            }

            return key.KeyChar;
        }

        public static bool IsKeyDown(int keyCode)
        {
            return (Win32.GetKeyState(keyCode) & 0x8000) == 0x8000;
        }
    }

    public static class Logger
    {
        public static void Info(string? message)
        {
            Console.WriteLine($"INF {DateTime.Now}: {message}");
        }

        public static void Debug(string? message)
        {
            Console.WriteLine($"DBG {DateTime.Now}: {message}");
        }

        public static void Warn(string? message)
        {
            Console.WriteLine($"WRN {DateTime.Now}: {message}");
        }

        public static void Error(string? message)
        {
            Console.WriteLine($"ERR {DateTime.Now}: {message}");
        }
    }

    [SuppressMessage("Major Code Smell", "S4200:Native methods should be wrapped", Justification = "Rather not wrap all the Win32 API functions.")]
    public static class Win32
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern short GetKeyState(int vKey);

        [DllImport("user32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool GetMessage(ref Message message, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct HookStruct { public uint vkCode; public uint scanCode; public uint flags; public uint time; public IntPtr dwExtraInfo; }

        [StructLayout(LayoutKind.Sequential)]
        public struct Message { public IntPtr hwnd; public uint message; public UIntPtr wParam; public IntPtr lParam; public uint time; public Point pt; }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point { public long x; public long y; }
    }
}
