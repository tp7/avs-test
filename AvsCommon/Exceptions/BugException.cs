using System;
using System.Runtime.Serialization;

namespace AvsCommon.Exceptions
{
    [Serializable]
    public class BugException : Exception
    {
        public BugException(string message) : base(message)
        {
        }

        protected BugException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}