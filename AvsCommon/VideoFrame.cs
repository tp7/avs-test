using System;
using AvsCommon.Enums;

namespace AvsCommon
{
    [Serializable]
    public abstract class VideoFrame
    {
        public abstract int GetHeight(Plane? plane = null);
        public abstract int GetWidth(Plane? plane = null);
        public abstract int GetPitch(Plane? plane = null);
        public abstract byte[] GetPlaneData(Plane? plane = null);
        public abstract Colorspace GetColorspace();

        public bool ColorspaceMatch(VideoFrame other)
        {
            return GetColorspace() == other.GetColorspace();
        }

        public bool DimensionsMatch(VideoFrame other)
        {
            return GetWidth() == other.GetWidth() &&
                   GetWidth(Plane.U) == other.GetWidth(Plane.U) &&
                   GetWidth(Plane.V) == other.GetWidth(Plane.V) &&
                   GetHeight() == other.GetHeight() &&
                   GetHeight(Plane.U) == other.GetHeight(Plane.U) &&
                   GetHeight(Plane.V) == other.GetHeight(Plane.V);
        }
    }
}