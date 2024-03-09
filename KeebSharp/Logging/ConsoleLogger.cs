using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace KeebSharp.Logging
{
    public class ConsoleLogger
    {
        public void Info(string message)
        {
            Console.WriteLine($"INFO:  {message}");
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
