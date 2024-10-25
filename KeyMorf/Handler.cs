using System;

namespace KeyMorf
{
    public class Handler
    {
        private static readonly IntPtr KEY_UP = new IntPtr(0x0101);
        private static readonly IntPtr KEY_DOWN = new IntPtr(0x0100);

        public bool Handle(IntPtr wParam, int keyCode)
        {
            if (wParam == KEY_UP)
            {
                Logger.Debug($"Key down with key code {keyCode}.");
            }

            if (wParam == KEY_DOWN)
            {
                Logger.Debug($"Key up with key code {keyCode}.");
            }

            return false;
        }
    }
}
