using System;
using System.Collections.Generic;
using AvsCommon.Enums;

namespace AvsCommon
{
    public enum TestKind
    {
        Correctness,
        Performance
    }

    [Serializable]
    public class TestCase
    {
        public string ImageName { get; set; }
        public string TestName { get; set; }
        public string ScriptText { get; set; }
        public int Frame { get; set; }
        public AccessType AccessType { get; set; }
        public TestKind Kind { get; set; }
        public int SkipFirst { get; set; }
        public int FrameCount { get; set; }
        public IReadOnlyCollection<TestParameter> Parameters { get; set; }
    }
}