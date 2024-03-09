using System;

namespace KeebSharp.Interop
{
    internal static class Hooks
    {
        public static IntPtr WH_KEYBOARD_LL = (IntPtr)13;
        public static IntPtr WM_KEYDOWN = (IntPtr)0x0100;
    }
}
