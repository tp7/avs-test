using System;

namespace AvsCommon.Ipc
{
    public enum TestResultKind
    {
        Frame,
        Fps,
        Exception
    }

    [Serializable]
    public class PerformanceData
    {
        public double Fps { get; set; }
        public long ElapsedMilliseconds { get; set; }
    }

    [Serializable]
    public class TestResult
    {
        public TestResultKind Kind { get; set; }
        public VideoFrame Frame { get; set; }
        public PerformanceData Performance { get; set; }
        public Exception Exception { get; set; }
    }
}