using System;

namespace KeebSharp.Interop
{
    internal static class Constants
    {
        public static IntPtr WH_KEYBOARD_LL = new (13);
        public static IntPtr WM_KEYDOWN = new (0x0100);
        public static IntPtr WM_KEYUP = new (0x0101);
    }
}
