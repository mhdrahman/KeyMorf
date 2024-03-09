using KeebSharp.Interop;
using System.Windows.Forms;

namespace KeebSharp.Input
{
    internal static class Keyboard
    {
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
            User32.keybd_event((byte)key, 0, 0, 0);
            User32.keybd_event((byte)key, 0, 2, 0);
            ReleaseShift(shift);
        }

        public static void KeyUp(Keys key)
        {
            User32.keybd_event((byte)key, 0, 2, 0);
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
