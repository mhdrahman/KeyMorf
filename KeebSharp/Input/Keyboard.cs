using KeebSharp.Interop;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeebSharp.Input
{
    internal static class Keyboard
    {
        public const int Flag = 89911140;

        public static void Press(params (Keys Key, bool Shift)[] keys)
        {
            foreach (var key in keys)
            {
                Press(key.Key, key.Shift);
            }
        }

        public static void Press(Keys key, bool shift)
        {
            HoldShift(shift);
            User32.keybd_event((byte)key, 0, 0, Flag);
            User32.keybd_event((byte)key, 0, 2, Flag);
            ReleaseShift(shift);
        }

        public static bool IsProgrammatic(IntPtr lParam)
        {
            var hookStructObject = Marshal.PtrToStructure(lParam, typeof(User32.HookStruct));
            if (hookStructObject == null)
            {
                return false;
            }

            var hookStruct = (User32.HookStruct)hookStructObject;
            return (int)hookStruct.dwExtraInfo == Flag;
        }

        public static bool KeyHeld(Keys key)
        {
            return (User32.GetKeyState((int)key) & 0x8000) > 0;
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
