using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.Serialization.Formatters.Binary;
using AvsCommon;
using AvsCommon.Ipc;

namespace AvsTest
{
    public class TestRunner
    {
        private readonly string _avsDllPath;

        public TestRunner(string avsDllPath)
        {
            _avsDllPath = avsDllPath;
        }

        public TestResult RunTestCase(TestCase test)
        {
            var fileName = Guid.NewGuid().ToString();
            var child = new Process
            {
                StartInfo = {FileName = "AvsInterop.exe", UseShellExecute = false, Arguments = fileName}
            };

            var fmt = new BinaryFormatter();
            
            using (var mmf = MemoryMappedFile.CreateNew(fileName, 10 * 1024 * 1024)) //random
            {
                using (var stream = mmf.CreateViewStream())
                {
                    fmt.Serialize(stream, new StartupData
                    {
                        TestCase = test,
                        AvsDllPath = _avsDllPath
                    });
                }

                child.Start();
                child.WaitForExit();

                using (var stream = mmf.CreateViewStream())
                {
                    return fmt.Deserialize(stream) as TestResult;
                }
            }
        }
    }
}