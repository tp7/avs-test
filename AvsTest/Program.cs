using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AvsCommon;
using AvsCommon.Ipc;
using AvsTest.Exceptions;

namespace AvsTest
{
    static class Program
    {
        static void Main(string[] args)
        {
            const string folder = @"D:\dev\cpp\avs\AvsTest\tests";
            const string stableAvsPath = @"D:\dev\avisynth\icl\avisynth.dll";
            const string testAvsPath = "avisynth.dll";
            RunTest(folder, stableAvsPath, testAvsPath);
        }

        private static void RunTest(string folder, string stableAvsPath, string testAvsPath)
        {
            var logger = new Logger();
            var scripts = LoadScipts(folder, logger);

            foreach (var script in scripts)
            {
                foreach (var testCase in script.GetTestCases())
                {
                    if (testCase.Parameters.Count == 0)
                    {
                        logger.LogTestStart(testCase.TestName);
                    }
                    else
                    {
                        logger.LogTestStart(string.Format("{0}: {1}", testCase.TestName,
                            string.Join(", ",
                                testCase.Parameters.Select(f => string.Format("{0}={1}", f.Name, f.Value)))));
                    }
                    

                    var testResult = new TestRunner(testAvsPath).RunTestCase(testCase);
                    var idealResult = new TestRunner(stableAvsPath).RunTestCase(testCase);

                    LogTestResult(testResult, idealResult, logger);

                    logger.WriteEmptyLine();
                }
            }
        }

        private static void LogTestResult(TestResult testResult, TestResult idealResult, Logger logger)
        {
            if (testResult.Kind == TestResultKind.Exception)
            {
                logger.LogTestFailure(string.Format("Failed. Exception of type {0}: {1}",
                    testResult.Exception.GetType().FullName, testResult.Exception.Message));
                return;
            }
            if (idealResult.Kind == TestResultKind.Exception)
            {
                logger.LogTestFailure(string.Format("Failed. Exception of type {0}: {1}",
                    idealResult.Exception.GetType().FullName, idealResult.Exception.Message));
                return;
            }

            var testFrame = testResult.Frame;
            var refFrame = idealResult.Frame;

            if (!testFrame.DimensionsMatch(refFrame))
            {
                logger.LogTestFailure("Failed. Frame dimensions don't match");
                return;
            }
            if (!testFrame.ColorspaceMatch(refFrame))
            {
                logger.LogTestFailure("Failed: colorspace doesn't match");
                return;
            }
            var diff = ImageFunctions.CompareImages(refFrame, testFrame);
            if (diff.AllZero)
            {
                logger.LogSuccess("Success");
            }
            else
            {
                logger.LogTestFailure("Failed: frames aren't identical");
            }
        }

        private static IEnumerable<TestScript> LoadScipts(string scriptsFolder, Logger logger)
        {
            if (!Directory.Exists(scriptsFolder))
            {
                logger.LogError(string.Format("Scripts directory doesn't exist: {0}", scriptsFolder));
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
                    logger.LogWarning(string.Format("Error parsing script {0}:{1}\t {2}", path, Environment.NewLine,
                        e.Message));
                }
            }
            return testScripts;
        }
    }
}
