using System;

namespace AvsCommon.Ipc
{
    [Serializable]
    public class StartupData
    {
        public TestCase TestCase { get; set; }
        public string AvsDllPath { get; set; }
    }
}