using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace AvsCommon.Exceptions
{
    [Serializable]
    public class AvsLoadingException : Exception
    {
        public AvsLoadingException(int win32Error, string path)
            : base(string.Format("Couldn't load avisynth at path {0}. Error code {1}: {2}", path, win32Error,
                new Win32Exception(win32Error).Message))
        {
        }

        protected AvsLoadingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}