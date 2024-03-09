using KeebSharp.Logging;

namespace KeebSharp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            logger.Info("Starting KeebSharp...");
        }
    }
}