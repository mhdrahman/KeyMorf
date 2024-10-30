using System;

namespace KeyMorf
{
    /// <summary>
    /// Simple console logger with a standard format.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// The level of logs that should be output, <see cref="LogLevel"/>.
        /// </summary>
        public static int Level { get; set; }

        /// <summary>
        /// Logs [INF] <paramref name="message"/> to the console if <see cref="Level"/>
        /// is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="message">Info message to be logged.</param>
        public static void Info(string? message)
        {
            if (Level < LogLevel.Info)
            {
                return;
            }

            Console.WriteLine($"[INF] {DateTime.Now:yyyy/MM/ddTHH:mm:ss:fff}: {message}");
        }

        /// <summary>
        /// Logs [DBG] <paramref name="message"/> to the console if <see cref="Level"/>
        /// is <see cref="LogLevel.Debug"/> or above.
        /// </summary>
        /// <param name="message">Debug message to be logged.</param>
        public static void Debug(string? message)
        {
            if (Level < LogLevel.Debug)
            {
                return;
            }

            Console.WriteLine($"[DBG] {DateTime.Now:yyyy/MM/ddTHH:mm:ss:fff}: {message}");
        }

        /// <summary>
        /// Logs the [ERR] <paramref name="message"/> to the console if <see cref="Level"/>
        /// is anything other than <see cref="LogLevel.None"/>. This exits the application!
        /// </summary>
        /// <param name="message">Error message to be logged.</param>
        public static void Error(string? message)
        {
            if (Level == LogLevel.None)
            {
                return;
            }

            Console.WriteLine($"[ERR] {DateTime.Now:yyyy/MM/ddTHH:mm:ss:fff}: {message}");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Contains constants indicative of logging level.
    /// </summary>
    public static class LogLevel
    {
        /// <summary>
        /// No logs should be output.
        /// </summary>
        public const int None = 0;

        /// <summary>
        /// Only logs written via <see cref="Logger.Info(string?)"/>
        /// or <see cref="Logger.Error(string?)"/> should be output.
        /// </summary>
        public const int Info = 1;

        /// <summary>
        /// All logs should be output.
        /// </summary>
        public const int Debug = 2;
    }
}
