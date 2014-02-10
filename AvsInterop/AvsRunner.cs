using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.Serialization.Formatters.Binary;
using AvsCommon;
using AvsCommon.Enums;
using AvsCommon.Exceptions;
using AvsCommon.Ipc;

namespace AvsInterop
{
    static class AvsRunner
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new BugException("Invalid number of arguments");
            }
            var fmt = new BinaryFormatter();
            try
            {
                using (var mmf = MemoryMappedFile.OpenExisting(args[0]))
                {
                    try
                    {
                        StartupData startData;
                        using (var stream = mmf.CreateViewStream())
                        {
                            startData = (StartupData)fmt.Deserialize(stream);
                        }
                        var result = RunTestCase(startData.AvsDllPath, startData.TestCase);

                        using (var stream = mmf.CreateViewStream())
                        {
                            fmt.Serialize(stream, result);
                        }
                    }
                    catch(Exception e)
                    {
                        using (var stream = mmf.CreateViewStream())
                        {
                            fmt.Serialize(stream, new TestResult
                            {
                                Exception = e,
                                Kind = TestResultKind.Exception
                            });
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Memory-mapped file does not exist. Run the wrong exe?");
            }
        }

        private static TestResult RunTestCase(string avsDllPath, TestCase test)
        {
            using (var env = new AvsScriptEnvironment(avsDllPath))
            {
                using (var evalResult = env.Invoke("eval", new AvsValue(test.ScriptText), true))
                {
                    if (evalResult.IsError)
                    {
                        throw new AvsOperationException(string.Format("AviSynth returned error: {0}",
                            evalResult.ErrorMessage));
                    }
                    using (var clip = evalResult.AsClip())
                    {
                        if (test.Kind == TestKind.Performance)
                        {
                            return new TestResult
                            {
                                Performance = GetPerformanceData(test, clip),
                                Kind = TestResultKind.Fps
                            };
                        }
                        return new TestResult
                        {
                            Frame = GetOutputFrame(test, clip),
                            Kind = TestResultKind.Frame
                        };
                    }
                }
            }
        }

        private static PerformanceData GetPerformanceData(TestCase test, AvsClip clip)
        {
            for (int i = 0; i < test.SkipFirst; i++)
            {
                clip.GetFrame(i).Dispose();
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < test.FrameCount; i++)
            {
                clip.GetFrame(i).Dispose();
                //todo: it's a VERY BAD IDEA to directly write to console from here
                if (i%100 == 0)
                {
                    RemoveCurrentConsoleLine();
                    Console.Write("Frame: {0}", i);
                }
            }
            RemoveCurrentConsoleLine();
            sw.Stop();
            return new PerformanceData
            {
                ElapsedMilliseconds = sw.ElapsedMilliseconds,
                Fps = test.FrameCount/sw.Elapsed.TotalSeconds
            };
        }

        private static void RemoveCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new String(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        private static VideoFrame GetOutputFrame(TestCase test, AvsClip clip)
        {
            if (test.AccessType == AccessType.Sequential)
            {
                //prefetch all frames before required
                for (int i = 0; i < test.Frame; i++)
                {
                    clip.GetFrame(i).Dispose();
                }
            }
            using (var frame = clip.GetFrame(test.Frame))
            {
                return new ManagedVideoFrame(frame);
            }
        }
    }
}
