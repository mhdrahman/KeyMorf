namespace KeyMorf
{
    public static class Keyboard
    {
        public static void Press(Keys key)
            => Win32.keybd_event((byte)key, 0, Win32.KEYEVENTF_KEYDOWN, 0);

        public static void Release(Keys key)
            => Win32.keybd_event((byte)key, 0, Win32.KEYEVENTF_KEYUP, 0);

        public static void SendKey(Keys key, params Keys[] mods)
        {
            foreach (var mod in mods)
            {
                Press(mod);
            }

            Press(key);
            Release(key);

            foreach (var mod in mods)
            {
                Release(mod);
            }
        }
    }
}
