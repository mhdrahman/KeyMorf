using System;

namespace KeebSharp.Logging
{
    /// <summary>
    /// Logger for logging to console.
    /// </summary>
    internal class ConsoleLogger
    {
        private readonly LogLevel _logLevel;

        /// <summary>
        /// Intitialises an instance of <see cref="ConsoleLogger"/>.
        /// </summary>
        /// <param name="logLevel">The desired <see cref="LogLevel"/>.</param>
        public ConsoleLogger(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        /// <summary>
        /// Log an error message.
        /// </summary>
        /// <param name="message">Message to be logged.</param>
        public void Error(string message)
        {
            Console.WriteLine($"ERROR: {message}");
        }

        /// <summary>
        /// Log an warning message.
        /// </summary>
        /// <param name="message">Message to be logged.</param>
        public void Warn(string message)
        {
            if (_logLevel < LogLevel.Warn)
            {
                return;
            }

            Console.WriteLine($"WARN:  {message}");
        }

        /// <summary>
        /// Log an info message.
        /// </summary>
        /// <param name="message">Message to be logged.</param>
        public void Info(string message)
        {
            if (_logLevel < LogLevel.Info)
            {
                return;
            }

            Console.WriteLine($"INFO:  {message}");
        }

        /// <summary>
        /// Log a debug message.
        /// </summary>
        /// <param name="message">Message to be logged.</param>
        public void Debug(string message)
        {
            if (_logLevel < LogLevel.Debug)
            {
                return;
            }

            Console.WriteLine($"DEBUG: {message}");
        }
    }
}
