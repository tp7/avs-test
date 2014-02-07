using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AvsCommon;
using AvsCommon.Ipc;
using AvsTest.Exceptions;
using CommandLine;
using CommandLine.Text;

namespace AvsTest
{
    internal class Options
    {
        [Option('r', "ref-dll", DefaultValue = "AviSynth.dll", HelpText="Path to reference AviSynth.dll")]
        public string StableAvs { get; set; }

        [Option('t', "test-dll", Required = true, HelpText="Path to AviSynth.dll under test")]
        public string TestAvs { get; set; }

        [Option('s', "scripts", Required = true, HelpText="Path to folder with test scripts")]
        public string TestScripsFolder { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("Avisynth black-box testing app", "v0.000000001"),
                Copyright = new CopyrightInfo("Victor Efimov", 2014),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine(
                @"Usage: AvsTest --stable-dll C:\stable\avisynth.dll --ref-dll C:\unstable\avisynth.dll --scripts C:\scripts");
            help.AddOptions(this);
            return help;
        }
    }

    static class Program
    {
        static void Main(string[] args)
        {
            var logger = new Logger();
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                RunTest(options.TestScripsFolder, options.StableAvs, options.TestAvs);
            }
            else
            {
                logger.LogError("Invalid commandline");
            }
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
            if (!testFrame.ColorspaceMatches(refFrame))
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
