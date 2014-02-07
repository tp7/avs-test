using System;
using System.Runtime.InteropServices;

namespace AvsInterop
{
    internal enum AvsValueType : short
    {
        Void = (short)'v',
        Clip = (short)'c',
        Bool = (short)'b',
        Int = (short)'i',
        Float = (short)'f',
        String = (short)'s',
        Array = (short)'a',
        Error = (short)'e',
    }


    [StructLayout(LayoutKind.Explicit, Size = 8, CharSet = CharSet.Ansi)]
    internal struct AvsValueStructure
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.I2)]
        internal AvsValueType type;
        [FieldOffset(2)]
        [MarshalAs(UnmanagedType.I2)]
        internal short arraySize;
        [FieldOffset(4)]
        internal IntPtr clipValue;
        [FieldOffset(4)]
        [MarshalAs(UnmanagedType.I1)]
        internal byte boolValue;
        [FieldOffset(4)]
        [MarshalAs(UnmanagedType.I4)]
        internal int intValue;
        [FieldOffset(4)]
        [MarshalAs(UnmanagedType.R4)]
        internal float floatValue;
        [FieldOffset(4)]
        internal IntPtr stringValue;
        [FieldOffset(4)]
        internal IntPtr arrayValue;
    }

    internal class AvsValue : IDisposable
    {
        private AvsValueStructure _value;
        private readonly bool _stringAllocated;
        private readonly AvsScriptEnvironment _env;

        internal string AsString()
        {
            if (_value.type != AvsValueType.String)
            {
                throw new InvalidOperationException();
            }
            return Marshal.PtrToStringAnsi(_value.stringValue);
        }

        internal AvsClip AsClip()
        {
            if (_value.type != AvsValueType.Clip)
            {
                throw new InvalidOperationException();
            }
            if (_env == null)
            {
                throw new InvalidOperationException();
            }
            return _env.AvsTakeClip(_value);
        }

        internal AvsValue(AvsValueStructure value, AvsScriptEnvironment env)
        {
            _value = value;
            _env = env;
        }

        internal AvsValueStructure GetAvsValue()
        {
            return _value;
        }

        internal AvsValue(string value)
        {
            _value = new AvsValueStructure
            {
                arraySize = 1,
                stringValue = Marshal.StringToHGlobalAnsi(value),
                type = AvsValueType.String
            };
            _stringAllocated = true;
        }

        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        ~AvsValue()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (_stringAllocated)
            {
                Marshal.FreeHGlobal(_value.stringValue);
            }
           // if (_value.type == AvsValueType.Clip)
           //{
           //     _env.AvsReleaseClip(_value.clipValue);
           // }
        }
    }
}