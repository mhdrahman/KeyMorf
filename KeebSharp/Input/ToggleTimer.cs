using System;
using System.Threading;

namespace KeebSharp.Input
{
    internal class ToggleTimer : IDisposable
    {
        private readonly Timer _timer;
        private bool _disposed;

        public ToggleTimer(TimerCallback callback)
        {
            _timer = new Timer(callback, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(25));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _timer.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
