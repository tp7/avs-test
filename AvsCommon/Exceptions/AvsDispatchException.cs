using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace AvsCommon.Exceptions
{
    [Serializable]
    public class AvsDispatchException : Exception
    {
        public AvsDispatchException(int win32Error, string functionName)
            : base(string.Format("Couldn't locate avisynth function {0}. Error code {1}: {2}", functionName, win32Error,
                new Win32Exception(win32Error).Message))
        {
        }

        protected AvsDispatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}