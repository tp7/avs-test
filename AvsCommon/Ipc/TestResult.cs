using System;

namespace AvsCommon.Ipc
{
    public enum TestResultKind
    {
        Frame,
        Exception
    }

    [Serializable]
    public class TestResult
    {
        public TestResultKind Kind { get; set; }
        public VideoFrame Frame { get; set; }
        public Exception Exception { get; set; }
    }
}