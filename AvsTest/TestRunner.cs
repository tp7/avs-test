using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
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
                    var result = (TestResult)fmt.Deserialize(stream);
                    if (result.Kind == TestResultKind.Fps)
                    {
                        result.Performance.PeakMemoryUsageInBytes = GetPeakMemoryUsage(child);
                    }
                    return result;
                }
            }
        }

        private static long GetPeakMemoryUsage(Process process)
        {
            ProcessMemoryCounters counters;
            if (!GetProcessMemoryInfo(process.Handle, out counters, Marshal.SizeOf(typeof (ProcessMemoryCounters))))
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }
            return (long)counters.PeakPagefileUsage;
        }


        [DllImport("psapi.dll", SetLastError = true)]
        private static extern bool GetProcessMemoryInfo(IntPtr hProcess, out ProcessMemoryCounters counters, int size);

        [StructLayout(LayoutKind.Sequential)]
        private struct ProcessMemoryCounters
        {
            public uint cb;
            public uint PageFaultCount;
            public UIntPtr PeakWorkingSetSize;
            public UIntPtr WorkingSetSize;
            public UIntPtr QuotaPeakPagedPoolUsage;
            public UIntPtr QuotaPagedPoolUsage;
            public UIntPtr QuotaPeakNonPagedPoolUsage;
            public UIntPtr QuotaNonPagedPoolUsage;
            public UIntPtr PagefileUsage;
            public UIntPtr PeakPagefileUsage;
        }

    }
}