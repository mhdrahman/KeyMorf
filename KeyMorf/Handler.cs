using System;
using System.Collections.Generic;
using System.Threading;

namespace KeyMorf
{
    public class Handler
    {
        private static readonly IntPtr KEY_UP = new IntPtr(0x0101);
        private static readonly IntPtr KEY_DOWN = new IntPtr(0x0100);

        private static Layer _testLayer = new Layer
        {
            Name = "Symbols",
            ToggleKey = Keys.Q,
            ToggleTimeMs = 200,
            Keymap = new Keymap
            {
                Mappings = new Dictionary<int, (int, int[])>
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
                    [Keys.P] = (Keys.LSquareBracket, new int[0]),
                    [Keys.Semicolon] = (Keys.RSquareBracket, new int[0]),

                    // !=&|
                    [Keys.N] = (Keys.One, new int[] { Keys.LShift }),
                    [Keys.M] = (Keys.Equal, new int[0]),
                    [Keys.Comma] = (Keys.Seven, new int[] { Keys.LShift }),
                    [Keys.Fullstop] = (Keys.Hash, new int[] { Keys.RControl, Keys.LAlt, Keys.LShift }),

                    // Nav
                    [Keys.H] = (Keys.Left, new int[0]),
                    [Keys.J] = (Keys.Down, new int[0]),
                    [Keys.K] = (Keys.Up, new int[0]),
                    [Keys.L] = (Keys.Right, new int[0]),
                },
            },
        };

        public bool Handle(IntPtr eventType, IntPtr lParam, int keyCode)
        {
            if (Keyboard.IsProgrammatic(lParam))
            {
                return false;
            }

            if (eventType == KEY_DOWN)
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

                // If the key is mapped, press the mapped key.
                if (_testLayer.Toggled && _testLayer.Keymap.Mappings.TryGetValue(keyCode, out var mapped))
                {
                    Keyboard.Press(mapped.KeyCode, mapped.Mods);
                    return true;
                }
            }

            if (eventType == KEY_UP && keyCode == _testLayer.ToggleKey)
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

            return false;
        }
    }

    public class Layer
    {
        public string Name { get; set; }

        public int ToggleKey { get; set; }

        public int ToggleTimeMs { get; set; }

        public bool Toggled { get; set; }

        public bool TogglePending { get; set; }

        public Keymap Keymap { get; set; }
    }

    public class Keymap
    {
        public Dictionary<int, (int KeyCode, int[] Mods)> Mappings { get; set; }

        public dynamic Macros { get; set; }
    }
}
