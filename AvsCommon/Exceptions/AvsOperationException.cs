using System;
using System.Runtime.Serialization;

namespace AvsCommon.Exceptions
{
    [Serializable]
    public class AvsOperationException : Exception
    {
        public AvsOperationException(string message) : base(message)
        {
        }

        protected AvsOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}