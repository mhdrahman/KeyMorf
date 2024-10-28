using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace KeyMorf
{
    public static class Handler
    {
        private static readonly Layer _testLayer = Keymap.Layers.First().Value;

        public static bool Handle(IntPtr wParam, IntPtr lParam, int keyCode)
        {
            // If this key was sent by us, we don't want to handle it again.
            var kbData = (Win32.HookStruct)Marshal.PtrToStructure(lParam, typeof(Win32.HookStruct))!;
            if ((kbData.flags & Win32.LLKHF_INJECTED) != 0)
            {
                return false;
            }

            var key = (Keys)keyCode;

            if (wParam == Win32.WM_KEYDOWN)
            {
                if (key == _testLayer.ToggleKey)
                {
                    // If there's no pending toggle and the layer is not already toggled, start the toggle timer.
                    if (!_testLayer.Toggled && !_testLayer.TogglePending)
                    {
                        Logger.Debug("Starting toggle timer.");
                        _testLayer.TogglePending = true;
                        new Timer(state =>
                        {
                            Logger.Debug("Toggle timer ended.");
                            if (_testLayer.TogglePending)
                            {
                                _testLayer.TogglePending = false;
                                _testLayer.Toggled = true;

                                Logger.Debug($"Layer {_testLayer.Name} toggled.");
                            }

                            // The timer is state.
                            var timer = (Timer?)state;
                            if (timer is not null)
                            {
                                timer.Dispose();
                                Logger.Debug("Timer disposed.");
                            }

                        }).Change(_testLayer.ToggleTimeMs, -1);
                    }

                    // Hold back toggle keys.
                    return true;
                }

                if (_testLayer.Toggled)
                {
                    // If the key is mapped, press the mapped key.
                    if (_testLayer.Mappings.TryGetValue(key, out var mapped))
                    {
                        Keyboard.SendKey(mapped.Key, mapped.Mods ?? Array.Empty<Keys>());
                        return true;
                    }

                    // TODO: If the key has a macro assigned, run it...
                }
            }

            // Only thing to do on key up is untoggle any layers associated with the key.
            if (wParam == Win32.WM_KEYUP && key == _testLayer.ToggleKey)
            {
                if (_testLayer.Toggled)
                {
                    _testLayer.Toggled = false;
                    Logger.Debug($"Layer {_testLayer.Name} untoggled.");

                    return true;
                }
                else if (_testLayer.TogglePending)
                {
                    _testLayer.TogglePending = false;
                    Logger.Debug($"Layer {_testLayer.Name} toggle cancelled.");

                    Keyboard.Press(_testLayer.ToggleKey);
                    return true;
                }
            }

            // For any unhandled keys, let them get processed by the next hook.
            return false;
        }
    }
}
