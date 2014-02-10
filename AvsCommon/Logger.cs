using System;
using System.Linq;
using AvsCommon.Enums;

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

        public void LogComparisonResult(ImageComparisonResult result)
        {
            if (result.AllZero)
            {
                Console.WriteLine("\tNo difference");
            }
            if (result.NumberOfPlanes == 1)
            {
                var pl = result.GetResult();
                Console.WriteLine("\tmax difference: {0}, SAD: {1}", pl.MaxDeviation, pl.Sad);
            }
            else if (result.NumberOfPlanes == 3)
            {
                foreach (var plane in Enum.GetValues(typeof (Plane)).Cast<Plane>())
                {
                    var pl = result.GetResult(plane);
                    Console.WriteLine("\tmax difference {0}: {1}, SAD: {2}", plane, pl.MaxDeviation, pl.Sad);
                }
            }
        }

        public void WriteEmptyLine()
        {
            Console.WriteLine();
        }

        public void LogEpilogue(int total, int failed, int success)
        {
            Console.WriteLine("Total: {0} tests", total);
            Console.WriteLine("Success: {0}", success);
            Console.WriteLine("Failed: {0}", failed);
        }
    }
}