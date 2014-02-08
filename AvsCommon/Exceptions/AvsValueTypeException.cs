using System;
using System.Runtime.Serialization;

namespace AvsCommon.Exceptions
{
    [Serializable]
    public class AvsValueTypeException : Exception
    {
        public AvsValueTypeException(string message) : base(message)
        {
        }

        protected AvsValueTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}