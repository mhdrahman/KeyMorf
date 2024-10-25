using System;

namespace KeyMorf
{
    public static class Logger
    {
        public static void Info(string? message)
            => Console.WriteLine($"INF {DateTime.Now}: {message}");

        public static void Debug(string? message)
            => Console.WriteLine($"DBG {DateTime.Now}: {message}");

        public static void Warn(string? message)
            => Console.WriteLine($"WRN {DateTime.Now}: {message}");

        public static void Error(string? message)
        {
            Console.WriteLine($"ERR {DateTime.Now}: {message}");
            Environment.Exit(1);
        }
    }
}
