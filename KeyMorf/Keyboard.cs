namespace KeyMorf
{
    public static class Keyboard
    {
        public static bool IsKeyDown(int keyCode)
            => (Win32.GetKeyState(keyCode) & 0x8000) == 0x8000;

        public static void Hold(int keyCode)
            => Win32.keybd_event((byte)keyCode, 0, 0, 0);

        public static void Release(int keyCode)
            => Win32.keybd_event((byte)keyCode, 0, 2, 0);

        public static void Press(int keyCode)
        {
            Win32.keybd_event((byte)keyCode, 0, 0, 0);
            Win32.keybd_event((byte)keyCode, 0, 2, 0);
        }
    }
}
