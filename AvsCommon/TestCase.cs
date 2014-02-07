using System;
using System.Collections.Generic;

namespace AvsCommon
{
    [Serializable]
    public class TestCase
    {
        public string TestName { get; set; }
        public string ScriptText { get; set; }
        public int Frame { get; set; }
        public AccessType AccessType { get; set; }
        public IReadOnlyCollection<TestParameter> Parameters { get; set; }
    }
}