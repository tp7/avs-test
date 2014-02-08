using System;
using System.Collections.Generic;
using AvsCommon.Enums;

namespace AvsCommon
{
    [Serializable]
    public class TestCase
    {
        public string ImageName { get; set; }
        public string TestName { get; set; }
        public string ScriptText { get; set; }
        public int Frame { get; set; }
        public AccessType AccessType { get; set; }
        public IReadOnlyCollection<TestParameter> Parameters { get; set; }
    }
}