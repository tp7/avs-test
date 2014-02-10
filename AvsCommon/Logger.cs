using System;
using System.Linq;
using AvsCommon.Enums;

namespace AvsCommon
{
    public static class Logger
    {
        public static void LogWarning(string message)
        {
            Console.WriteLine(message);
        }

        public static void LogError(string message)
        {
            Console.WriteLine(message);
        }

        public static void LogTestStart(string message)
        {
            Console.WriteLine(message);
        }

        public static void LogTestFailure(string message)
        {
            Console.WriteLine("\t{0}{1}", message, Environment.NewLine);
        }

        public static void LogComparisonTestResult(ImageComparisonResult result)
        {
            if (result.AllZero)
            {
                Console.WriteLine("\tPassed{0}", Environment.NewLine);
                return;
            }
            Console.WriteLine("\tFailed: frames aren't identical");
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
            else
            {
                throw new NotImplementedException("Weird number of planes");
            }
        }

        public static void LogEpilogue(int total, int failed, int success)
        {
            Console.WriteLine("Total: {0} tests", total);
            Console.WriteLine("Success: {0}", success);
            Console.WriteLine("Failed: {0}", failed);
        }
    }
}