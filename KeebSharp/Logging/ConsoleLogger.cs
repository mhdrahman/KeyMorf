using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace KeebSharp.Logging
{
    internal class ConsoleLogger
    {
        // TODO accept a log level in the constructor and respect it
        public enum LogLevel { Info, Debug, Warn, Error }

        public void Info(string message)
        {
            Console.WriteLine($"INFO:  {message}");
        }

        public void Debug(string message)
        {
            Console.WriteLine($"DEBUG: {message}");
        }

        public void Warn(string message)
        {
            Console.WriteLine($"WARN:  {message}");
        }

        public void Error(string message)
        {
            Console.WriteLine($"ERROR: {message}");
        }

        public void Win32()
        {
            Error($"{new Win32Exception(Marshal.GetLastWin32Error())}");
        }
    }
}
