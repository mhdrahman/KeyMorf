using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace KeyMorf
{
    public static class Handler
    {
        private static readonly Layer _testLayer = new("Symbols", new Keymap(
            new Dictionary<Keys, (Keys, Keys[])>
            {
                // (-)
                [Keys.Y] = (Keys.Nine, new Keys[] { Keys.LShift }),
                [Keys.U] = (Keys.Zero, new Keys[] { Keys.LShift }),

                // {-}
                [Keys.I] = (Keys.LSquareBracket, new Keys[] { Keys.LShift }),
                [Keys.O] = (Keys.RSquareBracket, new Keys[] { Keys.LShift }),

                // <->
                [Keys.Eight] = (Keys.Comma, new Keys[] { Keys.LShift }),
                [Keys.Nine] = (Keys.Fullstop, new Keys[] { Keys.LShift }),

                // [-]
                [Keys.P] = (Keys.LSquareBracket, Array.Empty<Keys>()),
                [Keys.Semicolon] = (Keys.RSquareBracket, Array.Empty<Keys>()),

                // !=&|
                [Keys.N] = (Keys.One, new Keys[] { Keys.LShift }),
                [Keys.M] = (Keys.Equal, Array.Empty<Keys>()),
                [Keys.Comma] = (Keys.Seven, new Keys[] { Keys.LShift }),
                [Keys.Fullstop] = (Keys.Hash, new Keys[] { Keys.RControl, Keys.LAlt, Keys.LShift }),

                // Nav
                [Keys.H] = (Keys.Left, Array.Empty<Keys>()),
                [Keys.J] = (Keys.Down, Array.Empty<Keys>()),
                [Keys.K] = (Keys.Up, Array.Empty<Keys>()),
                [Keys.L] = (Keys.Right, Array.Empty<Keys>()),
            },
            new Dictionary<Keys, (Keys, Keys[])[]>
            {
                [Keys.Up] = new (Keys, Keys[])[]
                {
                    (Keys.H, new Keys[] { Keys.LShift }),
                    (Keys.E, Array.Empty<Keys>()),
                    (Keys.L, Array.Empty<Keys>()),
                    (Keys.L, Array.Empty<Keys>()),
                    (Keys.O, Array.Empty<Keys>()),
                    (Keys.Comma, Array.Empty<Keys>()),
                    (Keys.Space, Array.Empty<Keys>()),
                    (Keys.W, new Keys[] { Keys.RShift }),
                    (Keys.O, Array.Empty<Keys>()),
                    (Keys.R, Array.Empty<Keys>()),
                    (Keys.L, Array.Empty<Keys>()),
                    (Keys.D, Array.Empty<Keys>()),
                    (Keys.One, new Keys[] { Keys.RShift }),
                }
            }))
        {
            ToggleKey = Keys.Z,
            ToggleTimeMs = 200,
        };

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
                    if (_testLayer.Keymap.Mappings.TryGetValue(key, out var mapped))
                    {
                        Keyboard.SendKey(mapped.KeyCode, mapped.Mods);
                        return true;
                    }

                    // If the key has a macro assigned, run it.
                    if (_testLayer.Keymap.Macros.TryGetValue(key, out var macro))
                    {
                        foreach (var (KeyCode, Mods) in macro)
                        {
                            Keyboard.SendKey(KeyCode, Mods);
                        }

                        return true;
                    }
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

    public class Layer
    {
        public string Name { get; }

        public Keys ToggleKey { get; set; }

        public int ToggleTimeMs { get; set; }

        public bool Toggled { get; set; }

        public bool TogglePending { get; set; }

        public Keymap Keymap { get; }

        public Layer(string name, Keymap keymap)
        {
            Name = name;
            Keymap = keymap;
        }
    }

    public class Keymap
    {
        public Dictionary<Keys, (Keys KeyCode, Keys[] Mods)> Mappings { get; }

        public Dictionary<Keys, (Keys KeyCode, Keys[] Mods)[]> Macros { get; }

        public Keymap(Dictionary<Keys, (Keys, Keys[])> mappings, Dictionary<Keys, (Keys, Keys[])[]> macros)
        {
            Mappings = mappings;
            Macros = macros;
        }
    }
}
