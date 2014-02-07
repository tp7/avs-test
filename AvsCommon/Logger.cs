using System;

namespace AvsCommon
{
    //one day I'll put something in here
    public class Logger
    {
        public void LogWarning(string message)
        {
            Console.WriteLine(message);
        }

        public void LogError(string message)
        {
            Console.WriteLine(message);
        }

        public void LogSuccess(string message)
        {
            Console.WriteLine("\t" + message);
        }

        public void LogTestStart(string message)
        {
            Console.WriteLine(message);
        }

        public void LogTestFailure(string message)
        {
            Console.WriteLine("\t" + message);
        }

        public void WriteEmptyLine()
        {
            Console.WriteLine();
        }
    }
}