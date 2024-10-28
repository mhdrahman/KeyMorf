using System;

namespace KeyMorf
{
    public static class Logger
    {
        public static int Level { get; set; }

        public static void Info(string? message)
        {
            if (Level < LogLevel.Info)
            {
                return;
            }

            Console.WriteLine($"[INF] {DateTime.Now:yyyy/MM/ddTHH:mm:ss:fff}: {message}");
        }

        public static void Debug(string? message)
        {
            if (Level < LogLevel.Debug)
            {
                return;
            }

            Console.WriteLine($"[DBG] {DateTime.Now:yyyy/MM/ddTHH:mm:ss:fff}: {message}");
        }

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

    public static class LogLevel
    {
        public const int None = 0;
        public const int Info = 1;
        public const int Debug = 2;
    }
}
