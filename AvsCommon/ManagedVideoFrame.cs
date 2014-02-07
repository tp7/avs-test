using System;
using System.ComponentModel;
using AvsCommon.Enums;

namespace AvsCommon
{
    [Serializable]
    public class ManagedVideoFrame : VideoFrame
    {
        private readonly byte[] _bufferY;
        private readonly byte[] _bufferU;
        private readonly byte[] _bufferV;

        private readonly int _width;
        private readonly int _widthChroma;
        private readonly int _height;
        private readonly int _heightChroma;
        private readonly int _pitch;
        private readonly int _pitchChroma;

        private readonly Colorspace _colorspace;

        public ManagedVideoFrame(VideoFrame frame)
        {
            _width = frame.GetWidth();
            _height = frame.GetHeight();
            _pitch = frame.GetPitch();
            _widthChroma = frame.GetWidth(Plane.U);
            _heightChroma = frame.GetHeight(Plane.U);
            _pitchChroma = frame.GetPitch(Plane.U);
            _bufferY = frame.GetPlaneData();
            _bufferU = frame.GetPlaneData(Plane.U);
            _bufferV = frame.GetPlaneData(Plane.V);
            _colorspace = frame.GetColorspace();
        }

        public ManagedVideoFrame(byte[] rgbBuffer, int widthInBytes, int height, int pitch, Colorspace colorspace)
        {
            _bufferY = rgbBuffer;
            _width = widthInBytes;
            _height = height;
            _pitch = pitch;
            _colorspace = colorspace;
        }

        public override int GetHeight(Plane? plane = null)
        {
            return plane == null || plane == Plane.Y ? _height : _heightChroma;
        }

        public override int GetWidth(Plane? plane = null)
        {
            return plane == null || plane == Plane.Y ? _width : _widthChroma;
        }

        public override int GetPitch(Plane? plane = null)
        {
            return plane == Plane.Y || plane == null ? _pitch : _pitchChroma;
        }

        public override byte[] GetPlaneData(Plane? plane = null)
        {
            switch (plane)
            {
                case Plane.Y:
                case null:
                    return _bufferY;
                case Plane.U:
                    return _bufferU;
                case Plane.V:
                    return _bufferV;
            }
            throw new InvalidEnumArgumentException();
        }

        public override Colorspace GetColorspace()
        {
            return _colorspace;
        }
    }
}