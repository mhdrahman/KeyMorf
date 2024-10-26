using System;
using System.Collections.Generic;
using System.Threading;

namespace KeyMorf
{
    public static class Handler
    {
        private static readonly IntPtr KEY_UP = new(0x0101);
        private static readonly IntPtr KEY_DOWN = new(0x0100);

        private static readonly Layer _testLayer = new("Symbols", new Keymap(
            new Dictionary<int, (int, int[])>
            {
                // (-)
                [Keys.Y] = (Keys.Nine, new int[] { Keys.LShift }),
                [Keys.U] = (Keys.Zero, new int[] { Keys.LShift }),

                // {-}
                [Keys.I] = (Keys.LSquareBracket, new int[] { Keys.LShift }),
                [Keys.O] = (Keys.RSquareBracket, new int[] { Keys.LShift }),

                // <->
                [Keys.Eight] = (Keys.Comma, new int[] { Keys.LShift }),
                [Keys.Nine] = (Keys.Fullstop, new int[] { Keys.LShift }),

                // [-]
                [Keys.P] = (Keys.LSquareBracket, Array.Empty<int>()),
                [Keys.Semicolon] = (Keys.RSquareBracket, Array.Empty<int>()),

                // !=&|
                [Keys.N] = (Keys.One, new int[] { Keys.LShift }),
                [Keys.M] = (Keys.Equal, Array.Empty<int>()),
                [Keys.Comma] = (Keys.Seven, new int[] { Keys.LShift }),
                [Keys.Fullstop] = (Keys.Hash, new int[] { Keys.RControl, Keys.LAlt, Keys.LShift }),

                // Nav
                [Keys.H] = (Keys.Left, Array.Empty<int>()),
                [Keys.J] = (Keys.Down, Array.Empty<int>()),
                [Keys.K] = (Keys.Up, Array.Empty<int>()),
                [Keys.L] = (Keys.Right, Array.Empty<int>()),
            },
            new Dictionary<int, (int, int[])[]>
            {
                [Keys.Up] = new (int, int[])[]
                {
                    (Keys.H, new int[] { Keys.LShift }),
                    (Keys.E, Array.Empty<int>()),
                    (Keys.L, Array.Empty<int>()),
                    (Keys.L, Array.Empty<int>()),
                    (Keys.O, Array.Empty<int>()),
                    (Keys.Comma, Array.Empty<int>()),
                    (Keys.Space, Array.Empty<int>()),
                    (Keys.W, new int[] { Keys.RShift }),
                    (Keys.O, Array.Empty<int>()),
                    (Keys.R, Array.Empty<int>()),
                    (Keys.L, Array.Empty<int>()),
                    (Keys.D, Array.Empty<int>()),
                    (Keys.One, new int[] { Keys.RShift }),
                }
            }))
        {
            ToggleKey = Keys.Q,
            ToggleTimeMs = 200,
        };

        public static bool Handle(IntPtr wParam, IntPtr lParam, int keyCode)
        {
            // If this key was sent by us, we don't want to handle it again.
            if (Keyboard.IsProgrammatic(lParam))
            {
                return false;
            }

            if (wParam == KEY_DOWN)
            {
                if (keyCode == _testLayer.ToggleKey)
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
                    if (_testLayer.Keymap.Mappings.TryGetValue(keyCode, out var mapped))
                    {
                        Keyboard.Press(mapped.KeyCode, mapped.Mods);
                        return true;
                    }

                    // If the key has a macro assigned, run it.
                    if (_testLayer.Keymap.Macros.TryGetValue(keyCode, out var macro))
                    {
                        foreach (var (KeyCode, Mods) in macro)
                        {
                            Keyboard.Press(KeyCode, Mods);
                        }

                        return true;
                    }
                }
            }

            // Only thing to do on key up is untoggle any layers associated with the key.
            if (wParam == KEY_UP && keyCode == _testLayer.ToggleKey)
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

        public int ToggleKey { get; set; }

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
        public Dictionary<int, (int KeyCode, int[] Mods)> Mappings { get; }

        public Dictionary<int, (int KeyCode, int[] Mods)[]> Macros { get; }

        public Keymap(Dictionary<int, (int, int[])> mappings, Dictionary<int, (int, int[])[]> macros)
        {
            Mappings = mappings;
            Macros = macros;
        }
    }
}
