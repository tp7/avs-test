namespace AvsCommon.Enums
{
    //from avisynth.h 2.6
    // Specific colorformats
    public enum Colorspace
    {
        BGR = 1 << 28,
        YUV = 1 << 29,
        INTERLEAVED = 1 << 30,
        PLANAR = 1 << 31,

        Sub_Width_1 = 3, // YV24
        Sub_Width_2 = 0, // YV12, I420, YV16
        Sub_Width_4 = 1, // YUV9, YV411

        VPlaneFirst = 1 << 3, // YV12, YV16, YV24, YV411, YUV9
        UPlaneFirst = 1 << 4, // I420

        Sub_Height_1 = 3 << 8, // YV16, YV24, YV411
        Sub_Height_2 = 0 << 8, // YV12, I420
        Sub_Height_4 = 1 << 8, // YUV9

        // Specific colorformats
        UNKNOWN = 0,
        BGR24 = 1 << 0 | BGR | INTERLEAVED,
        BGR32 = 1 << 1 | BGR | INTERLEAVED,
        YUY2 = 1 << 2 | YUV | INTERLEAVED,
        RAW32 = 1 << 5 | INTERLEAVED,

        YV24 = PLANAR | YUV | VPlaneFirst | Sub_Height_1 | Sub_Width_1,
        // YUV 4:4:4 planar
        YV16 = PLANAR | YUV | VPlaneFirst | Sub_Height_1 | Sub_Width_2,
        // YUV 4:2:2 planar
        YV12 = PLANAR | YUV | VPlaneFirst | Sub_Height_2 | Sub_Width_2,
        // y-v-u, 4:2:0 planar
        I420 = PLANAR | YUV | UPlaneFirst | Sub_Height_2 | Sub_Width_2,
        // y-u-v, 4:2:0 planar
        IYUV = I420,
        YUV9 = PLANAR | YUV | VPlaneFirst | Sub_Height_4 | Sub_Width_4,
        // YUV 4:1:0 planar
        YV411 = PLANAR | YUV | VPlaneFirst | Sub_Height_1 | Sub_Width_4,
        // YUV 4:1:1 planar

        Y8 = PLANAR | INTERLEAVED | YUV, // Y   4:0:0 planar
    };
}