using KeebSharp.Input;
using KeebSharp.Interop;
using KeebSharp.Logging;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeebSharp.Handlers
{
    /// <summary>
    /// Handler for the keyboard events.
    /// </summary>
    internal class Handler
    {
        private ToggleTimer? Timer;
        private Layer? ActiveLayer;

        private readonly ConsoleLogger _logger;

        /// <summary>
        /// Initialises an instance of <see cref="Handler"/>.
        /// </summary>
        /// <param name="logger"><see cref="ConsoleLogger"/></param>
        public Handler(ConsoleLogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handle the keyboard event.
        /// </summary>
        /// <param name="wParam">Event type.</param>
        /// <param name="lParam">Virtual key code of the key associated with the event.</param>
        /// <returns>A <see cref="bool"/> indicating whether the event was handled or not.</returns>
        public bool Handle(IntPtr wParam, IntPtr lParam)
        {
            var inputKey = (Keys)Marshal.ReadInt32(lParam);

            RefreshLayerState(wParam, inputKey);
            return HandleKeyDown(wParam, inputKey);
        }

        /// <summary>
        /// Refresh the layer state. Checks if any key up events should disable a toggle.
        /// </summary>
        /// <param name="wParam">Event type.</param>
        /// <param name="inputKey">Key associated with the event.</param>
        private void RefreshLayerState(IntPtr wParam, Keys inputKey)
        {
            // Nothing to handle if it's not a key up event
            if (wParam != Constants.WM_KEYUP)
            {
                return;
            }

            // Nothing to handle if we have no layers set to activate with this key
            if (!Mapping.Layers.TryGetValue(inputKey, out var layer))
            {
                return;
            }

            layer.ToggleKeyHeld = false;
            if (layer.Active)
            {
                layer.Active = false;
                ActiveLayer = null;
                _logger.Info($"{layer.Name} layer inactive.");
            }    
        }

        /// <summary>
        /// Handle the key down event. This is where mapped keys are output.
        /// </summary>
        /// <param name="wParam">Event type.</param>
        /// <param name="inputKey">Key associated with the event.</param>
        /// <returns>A <see cref="bool"/> indicating whether the key down event was handled or not.</returns>
        private bool HandleKeyDown(IntPtr wParam, Keys inputKey)
        {
            // Nothing to handle if it's not a key down event
            if (wParam != Constants.WM_KEYDOWN)
            {
                return false;
            }

            if (Mapping.Layers.TryGetValue(inputKey, out var layer))
            {
                // This releases the toggle key to be output
                if (layer.ToggleKeyDisabled)
                {
                    layer.ToggleKeyDisabled = false;
                    return false;
                }

                layer.ToggleKeyHeld = true;
                void TimerHook(object? state)
                {
                    if (layer.ToggleKeyHeld)
                    {
                        // If the key has not been released when the timer expires, toggle the layer
                        layer.Active = true;
                        ActiveLayer = layer;
                        _logger.Info($"{layer.Name} layer active.");
                    }
                    else
                    {
                        // Otherwise, let the key through
                        layer.ToggleKeyDisabled = true;
                        Keyboard.Press(layer.ToggleKey, false);
                    }

                    Timer!.Dispose();
                }

                if (!layer.Active)
                {
                    Timer = new ToggleTimer(TimerHook);
                }

                // Hold back the key for now, if it doesn't toggle a layer it will be let through 
                return true;
            }

            if (ActiveLayer != null && ActiveLayer.Active && ActiveLayer.Mappings!.TryGetValue(inputKey, out var mappedKeys))
            {
                Keyboard.Press(mappedKeys);
                return true;
            }

            // Let all unhandled keys be processed normally
            return false;
        }
    }
}
