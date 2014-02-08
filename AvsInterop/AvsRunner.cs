using System;
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
                            fmt.Serialize(stream, new TestResult
                            {
                                Frame = result,
                                Kind = TestResultKind.Frame
                            });
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

        private static VideoFrame RunTestCase(string avsDllPath, TestCase test)
        {
            using (var env = new AvsScriptEnvironment(avsDllPath))
            {
                using (var result = env.Invoke("eval", new AvsValue(test.ScriptText), true))
                {
                    if (result.IsError)
                    {
                        throw new AvsOperationException(string.Format("AviSynth returned error: {0}",
                            result.ErrorMessage));
                    }
                    using (var clip = result.AsClip())
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
        }
    }
}
