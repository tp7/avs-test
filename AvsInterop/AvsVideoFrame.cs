using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using AvsCommon;

namespace AvsInterop
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct AvsVideoFrameBufferStructure
    {
        internal IntPtr data;
        private int data_size;
        private int sequence_number;
        private int refcount;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AvsVideoFrameStructure
    {
        private int refcount;
        public IntPtr vfb;
        public int offset;
        public int pitch;
        public int row_size;
        public int height;
        public int offsetU;
        public int offsetV;
        public int pitchUV;
        public int row_sizeUV;
        public int heightUV;
    }

    internal class AvsVideoFrame : VideoFrame, IDisposable
    {
        private readonly AvsVideoFrameStructure _value;
        private readonly AvsVideoFrameBufferStructure _frameBuffer;
        private readonly IntPtr _frame;
        private readonly AvsScriptEnvironment _env;

        //cached managed memory
        private byte[] _bufferY;
        private byte[] _bufferU;
        private byte[] _bufferV;
        private readonly Colorspace _colorspace;

        internal AvsVideoFrame(IntPtr framePtr, Colorspace colorspace, AvsScriptEnvironment env)
        {
            _frame = framePtr;
            _env = env;
            _colorspace = colorspace;
            _value = (AvsVideoFrameStructure)Marshal.PtrToStructure(_frame, typeof (AvsVideoFrameStructure));
            _frameBuffer = (AvsVideoFrameBufferStructure)Marshal.PtrToStructure(_value.vfb,
                typeof (AvsVideoFrameBufferStructure));
        }

        public override int GetHeight(Plane? plane = null)
        {
            if (plane == Plane.U || plane == Plane.V)
            {
                return _value.pitchUV != 0 ? _value.heightUV : 0;
            }
            return _value.height;
        }

        public override int GetWidth(Plane? plane = null)
        {
            if (plane == Plane.U || plane == Plane.V)
            {
                return _value.pitchUV != 0 ? _value.row_sizeUV : 0;
            }
            return _value.row_size;
        }

        public override int GetPitch(Plane? plane = null)
        {
            return plane == Plane.Y || plane == null ? _value.pitch : _value.pitchUV;
        }

        public override byte[] GetPlaneData(Plane? plane = null)
        {
            if (GetPitch(plane) == 0) //no data for this plane
            {
                return null;
            }
            int dataSize = GetHeight(plane)*GetPitch(plane);

            switch (plane)
            {
                case Plane.Y:
                case null:
                    if (_bufferY == null)
                    {
                        _bufferY = new byte[dataSize];
                        Marshal.Copy(GetReadPtr(plane), _bufferY, 0, dataSize);
                    }
                    return _bufferY;
                case Plane.U:
                    if (_bufferU == null)
                    {
                        _bufferU = new byte[dataSize];
                        Marshal.Copy(GetReadPtr(plane), _bufferU, 0, dataSize);
                    }
                    return _bufferU;
                case Plane.V:
                    if (_bufferV == null)
                    {
                        _bufferV = new byte[dataSize];
                        Marshal.Copy(GetReadPtr(plane), _bufferV, 0, dataSize);
                    }
                    return _bufferV;
            }
            throw new InvalidEnumArgumentException();
        }

        public override Colorspace GetColorspace()
        {
            return _colorspace;
        }

        private int GetOffset(Plane? plane = null)
        {
            switch (plane)
            {
                case Plane.U:
                    return _value.offsetU;
                case Plane.V:
                    return _value.offsetV;
                default:
                    return _value.offset;
            }
        }

        private IntPtr GetReadPtr(Plane? plane = null)
        {
            return _frameBuffer.data + GetOffset(plane);
        }

        ~AvsVideoFrame()
        {
            _env.AvsReleaseVideoFrame(_frame);
        }

        public void Dispose()
        {
            _env.AvsReleaseVideoFrame(_frame);
            GC.SuppressFinalize(this);
        }
    }
}