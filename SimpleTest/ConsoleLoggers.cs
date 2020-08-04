using System;

namespace SimpleTest
{
    public static class ConsoleLoggers
    {
        private static object _logLatch = new Object();
        public static void LogToConsole(string message)
        {

            lock(_logLatch)
                Console.WriteLine($"    |> {message}");
        }
        
        public static void LogExceptionToConsole(Exception e)
        {
            lock(_logLatch)
            {
                Console.Error.WriteLineAsync("==============================================================================================================================");
                Console.Error.WriteLineAsync(e.Message);
                if (!string.IsNullOrEmpty(e.StackTrace))
                    Console.Error.WriteLineAsync(e.StackTrace);
                else
                    Console.Error.WriteLineAsync(e.InnerException.StackTrace);
                Console.Error.WriteLineAsync("==============================================================================================================================");
            }

            Console.ReadLine();
        }
    }
}