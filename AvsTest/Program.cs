using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AvsCommon;
using AvsCommon.Ipc;
using AvsTest.Exceptions;
using CommandLine;

namespace AvsTest
{
    static class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                RunTest(options);
            }
            else
            {
                Logger.LogError("Invalid commandline");
            }
        }

        private static void RunTest(Options options)
        {
            var scripts = LoadScipts(options.TestScripsFolder);

            if (options.Exclude != null && options.Exclude.Any())
            {
                scripts = scripts.Where(f => options.Exclude.All(e =>
                    !string.Equals(e, f.Name, StringComparison.OrdinalIgnoreCase)
                    ));
            }
            if (options.Include != null && options.Include.Any())
            {
                scripts = scripts.Where(f => options.Include.Any(e =>
                    string.Equals(e, f.Name, StringComparison.OrdinalIgnoreCase)
                    ));
            }

            int total = 0;
            int failed = 0;
            int success = 0;

            foreach (var script in scripts)
            {
                foreach (var testCase in script.GetTestCases())
                {
                    if (testCase.Parameters.Count == 0)
                    {
                        Logger.LogTestStart(testCase.TestName);
                    }
                    else
                    {
                        Logger.LogTestStart(string.Format("{0}: {1}", testCase.TestName,
                            string.Join(", ",
                                testCase.Parameters.Select(f => string.Format("{0}={1}", f.Name, f.Value)))));
                    }
                    

                    var testResult = new TestRunner(options.TestAvs).RunTestCase(testCase);
                    var idealResult = new TestRunner(options.StableAvs).RunTestCase(testCase);

                    var result = LogTestResult(testResult, idealResult);
                    if (result)
                    {
                        success++;
                    }
                    else
                    {
                        failed++;
                    }
                    total++;

                    Logger.WriteEmptyLine();
                }
            }
            Logger.LogEpilogue(total, failed, success);
        }

        private static bool LogTestResult(TestResult testResult, TestResult idealResult)
        {
            if (testResult.Kind == TestResultKind.Exception)
            {
                Logger.LogTestFailure(string.Format("Failed. Exception of type {0}: {1}",
                    testResult.Exception.GetType().FullName, testResult.Exception.Message));
                return false;
            }
            if (idealResult.Kind == TestResultKind.Exception)
            {
                Logger.LogTestFailure(string.Format("Failed. Exception of type {0}: {1}",
                    idealResult.Exception.GetType().FullName, idealResult.Exception.Message));
                return false;
            }

            var testFrame = testResult.Frame;
            var refFrame = idealResult.Frame;

            if (!testFrame.DimensionsMatch(refFrame))
            {
                Logger.LogTestFailure("Failed. Frame dimensions don't match");
                return false;
            }
            if (!testFrame.ColorspaceMatches(refFrame))
            {
                Logger.LogTestFailure("Failed: colorspace doesn't match");
                return false;
            }
            var diff = ImageFunctions.CompareImages(refFrame, testFrame);
            if (diff.AllZero)
            {
                Logger.LogSuccess("Success");
                return true;
            }
            Logger.LogTestFailure("Failed: frames aren't identical");
            Logger.LogComparisonResult(diff);
            return false;
        }

        private static IEnumerable<TestScript> LoadScipts(string scriptsFolder)
        {
            if (!Directory.Exists(scriptsFolder))
            {
                Logger.LogError(string.Format("Scripts directory doesn't exist: {0}", scriptsFolder));
                return new List<TestScript>();
            }
            var paths = Directory.EnumerateFiles(scriptsFolder, "*.avs", SearchOption.AllDirectories);
            var testScripts = new List<TestScript>();

            foreach (var path in paths)
            {
                try
                {
                    var script = new TestScript(path);
                    testScripts.Add(script);
                }
                catch (ParsingException e)
                {
                    Logger.LogWarning(string.Format("Error parsing script {0}:{1}\t {2}", path, Environment.NewLine,
                        e.Message));
                }
            }
            return testScripts;
        }
    }
}
