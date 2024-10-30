using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace KeyMorf
{
    /// <summary>
    /// Class containing the logic for handling the keyboard input events.
    /// </summary>
    public static class Handler
    {
        /// <summary>
        /// The currently toggled <see cref="Layer"/>, null if no layer is currently toggled.
        /// </summary>
        private static Layer? _toggledLayer;

        /// <summary>
        /// Try to handle the keyboard input event. 
        /// </summary>
        /// <param name="wParam">The identifier of the keyboard message.</param>
        /// <param name="lParam">A pointer to a <see cref="Win32.HookStruct"/>.</param>
        /// <param name="keyCode">Marshalled <see cref="int"/> value of <paramref name="lParam"/>, this is the keycode of the key.</param>
        /// <returns>True if the event is handled, otherwise false.</returns>
        public static bool Handle(IntPtr wParam, IntPtr lParam, int keyCode)
        {
            // If this key was sent by us, we don't want to handle it again.
            var kbData = (Win32.HookStruct)Marshal.PtrToStructure(lParam, typeof(Win32.HookStruct))!;
            if ((kbData.flags & Win32.LLKHF_INJECTED) != 0)
            {
                return false;
            }

            var key = (Keys)keyCode;
            var isToggleKey = Keymap.Layers.TryGetValue(key, out var layer);

            if (wParam == Win32.WM_KEYDOWN)
            {
                if (isToggleKey)
                {
                    // If there's no pending toggle and the layer is not already toggled, start the toggle timer.
                    if (!layer!.Toggled && !layer.TogglePending)
                    {
                        Logger.Debug("Starting toggle timer.");
                        layer.TogglePending = true;
                        new Timer(state =>
                        {
                            Logger.Debug("Toggle timer ended.");
                            if (layer.TogglePending)
                            {
                                layer.TogglePending = false;
                                layer.Toggled = true;

                                _toggledLayer = layer;

                                Logger.Debug($"Layer {layer.Name} toggled.");
                            }

                            // The timer is state.
                            var timer = (Timer?)state;
                            if (timer is not null)
                            {
                                timer.Dispose();
                                Logger.Debug("Timer disposed.");
                            }

                        }).Change(layer.ToggleTimeMs, -1);
                    }

                    // Hold back toggle keys.
                    return true;
                }

                if (_toggledLayer is not null)
                {
                    // If the key is mapped, press the mapped key.
                    if (_toggledLayer.Mappings.TryGetValue(key, out var mapped))
                    {
                        Keyboard.SendKey(mapped.Key, mapped.Mods ?? Array.Empty<Keys>());
                        return true;
                    }

                    // If the key has a macro assigned, run it.
                    if (_toggledLayer.Macros.TryGetValue(key, out var macro))
                    {
                        foreach (var (Key, Mods) in macro)
                        {
                            Keyboard.SendKey(Key, Mods ?? Array.Empty<Keys>());
                        }

                        return true;
                    }
                }
            }

            // Only thing to do on key up is untoggle any layers or cancel any pending toggles associated with the key.
            if (wParam == Win32.WM_KEYUP && isToggleKey)
            {
                if (layer!.Toggled)
                {
                    layer.Toggled = false;
                    _toggledLayer = null;

                    Logger.Debug($"Layer {layer.Name} untoggled.");
                    return true;
                }
                else if (layer.TogglePending)
                {
                    layer.TogglePending = false;
                    Logger.Debug($"Layer {layer.Name} toggle cancelled.");

                    // If the toggle was pending but was cancelled, release the toggle key as a key press.
                    Keyboard.SendKey(layer.ToggleKey);
                    return true;
                }
            }

            // For any unhandled keys, let them get processed by the next hook.
            return false;
        }
    }
}
