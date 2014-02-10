using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AvsCommon;
using AvsCommon.Exceptions;
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
                RunTests(options);
            }
            else
            {
                Logger.LogError("Invalid commandline");
            }
        }

        private static void RunTests(Options options)
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
            int passed = 0;

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

                    bool success = RunComparisonTest(options, testCase);
                  
                    if (success)
                    {
                        passed++;
                    }
                    else
                    {
                        failed++;
                    }
                    total++;
                }
            }
            Logger.LogEpilogue(total, failed, passed);
        }

        private static bool RunComparisonTest(Options options, TestCase testCase)
        {
            var testResult = new TestRunner(options.TestAvs).RunTestCase(testCase);
            var idealResult = new TestRunner(options.StableAvs).RunTestCase(testCase);

            return LogTestResult(testResult, idealResult, options);
        }

        private static bool LogTestResult(TestResult testResult, TestResult idealResult, Options options)
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

            if (testResult.Kind != idealResult.Kind)
            {
                throw new BugException("Results of different kind in a single comparison test");
            }
            if (testResult.Kind == TestResultKind.Fps)
            {
                return LogFpsComparison(testResult, idealResult, options.MaxFpsDrop);
            }
            return LogFrameComparison(testResult, idealResult);
        }

        private static bool LogFpsComparison(TestResult test, TestResult reference, float maxFpsDrop)
        {
            bool success = (reference.Performance.Fps - test.Performance.Fps)/reference.Performance.Fps*100 < maxFpsDrop;
            Logger.LogFpsTestResult(success, test, reference);
            return success;
        }

        private static bool LogFrameComparison(TestResult test, TestResult reference)
        {
            var testFrame = test.Frame;
            var refFrame = reference.Frame;

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
            Logger.LogComparisonTestResult(diff);
            return diff.AllZero;
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
