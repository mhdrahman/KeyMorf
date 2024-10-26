using System;
using System.Runtime.InteropServices;

namespace KeyMorf
{
    public static class Keyboard
    {
        private const int ProgrammaticFlag = 89911140;

        public static bool IsKeyDown(int keyCode)
            => (Win32.GetKeyState(keyCode) & 0x8000) == 0x8000;

        public static void Hold(int keyCode)
            => Win32.keybd_event((byte)keyCode, 0, 0, ProgrammaticFlag);

        public static void Release(int keyCode)
            => Win32.keybd_event((byte)keyCode, 0, 2, ProgrammaticFlag);

        public static void Press(int keyCode, params int[] mods)
        {
            foreach (var mod in mods)
            {
                Hold(mod);
            }

            Win32.keybd_event((byte)keyCode, 0, 0, ProgrammaticFlag);
            Win32.keybd_event((byte)keyCode, 0, 2, ProgrammaticFlag);

            foreach (var mod in mods)
            {
                Release(mod);
            }
        }

        public static bool IsProgrammatic(IntPtr lParam)
        {
            var hookStructObject = Marshal.PtrToStructure(lParam, typeof(Win32.HookStruct));
            if (hookStructObject == null)
            {
                return false;
            }

            var hookStruct = (Win32.HookStruct)hookStructObject;
            return (int)hookStruct.dwExtraInfo == ProgrammaticFlag;
        }
    }
}
