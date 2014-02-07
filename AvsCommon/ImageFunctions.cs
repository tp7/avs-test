using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using AvsCommon.Exceptions;

namespace AvsCommon
{
    public class PlaneComparisonResult
    {
        public long Sad { get; set; }
        public long MaxDeviation { get; set; }
    }

    public class ImageComparisonResult
    {
        private readonly List<PlaneComparisonResult> _results = new List<PlaneComparisonResult> {null, null, null};

        public void SetResult(PlaneComparisonResult result, Plane? plane = null)
        {
            _results[(int?)plane ?? 0] = result;
        }

        public PlaneComparisonResult GetResult(Plane? plane = null)
        {
            return _results[(int?)plane ?? 0];
        }

        public bool AllZero { get { return _results.All(f => f == null || (f.MaxDeviation == 0 && f.Sad == 0)); } }
    }

    public static class ImageFunctions
    {
        public static ImageComparisonResult CompareImages(VideoFrame a, VideoFrame b)
        {
            if (a.GetColorspace() != b.GetColorspace())
            {
                throw new BugException("Colorspace doesn't match in CompareImages. This is a bug");
            }
            
            var result = new ImageComparisonResult();
            foreach (var plane in Enum.GetValues(typeof (Plane)).Cast<Plane>())
            {
                if (a.GetPitch(plane) != 0)
                {
                    var width = a.GetWidth(plane);
                    var height = a.GetHeight(plane);
                    if (width != b.GetWidth(plane) || height != b.GetHeight(plane))
                    {
                        throw new BugException("Images dimensions don't match in CompareImages. This is a bug");
                    }
                    var compResult = ComparePlane(a.GetPlaneData(plane), b.GetPlaneData(plane), width,
                        a.GetHeight(plane), a.GetPitch(plane), b.GetPitch(plane));
                    result.SetResult(compResult, plane);
                }
            }
            return result;
        }

        private static PlaneComparisonResult ComparePlane(byte[] a, byte[] b, int width, int height, int pitchA, int pitchB)
        {
            long sad = 0;
            long maxDev = 0;
            long offsetA = 0;
            long offsetB = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var diff = Math.Abs(a[offsetA+x] - b[offsetB+x]);
                    sad += diff;
                    maxDev = Math.Max(maxDev, diff);
                }
                offsetA += pitchA;
                offsetB += pitchB;
            }
            return new PlaneComparisonResult
            {
                MaxDeviation = maxDev,
                Sad = sad
            };
        }

        public static void SaveToRgb32File(VideoFrame frame, string path)
        {
            using (var bmp = AvsFrameToBitmap(frame))
            {
                bmp.Save(path, ImageFormat.Bmp);
            }
        }

        private static Bitmap AvsFrameToBitmap(VideoFrame frame)
        {
            switch (frame.GetColorspace())
            {
                case Colorspace.BGR24:
                    return BgrToBitmap(frame, 3);
                case Colorspace.BGR32:
                    return BgrToBitmap(frame, 4);
                case Colorspace.YV24:
                    return BgrToBitmap(ConvertYv24ToRgb32(frame), 4);
                default:
                    throw new NotImplementedException();
            }
        }

        private static Bitmap BgrToBitmap(VideoFrame frame, int bpp)
        {
            int width = frame.GetWidth();
            int pixelWidth = width / bpp;
            int pixelHeight = frame.GetHeight();
            int pitch = frame.GetPitch();

            var bmp = new Bitmap(pixelWidth, pixelHeight, bpp == 4 ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);

            var bmpData = bmp.LockBits(new Rectangle(0, 0, pixelWidth, pixelHeight), ImageLockMode.WriteOnly,
                bmp.PixelFormat);

            var data = frame.GetPlaneData();
            for (int i = 0; i < bmp.Height; i++)
            {
                Marshal.Copy(data, pitch * (pixelHeight - i - 1), bmpData.Scan0 + bmpData.Stride * i, pixelWidth * bpp);
            }
            //Unlock the pixels
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        private static byte ClipByte(int value)
        {
            if (value < 0)
            {
                return 0;
            }
            if (value > 255)
            {
                return 255;
            }
            return (byte)value;
        }

        private static VideoFrame ConvertYv24ToRgb32(VideoFrame frame)
        {
            var matrix = BuildMatrix(0.299, 0.114, 219, 112, 16, 13);

            var srcY = frame.GetPlaneData(Plane.Y);
            var srcU = frame.GetPlaneData(Plane.U);
            var srcV = frame.GetPlaneData(Plane.V);
            int width = frame.GetWidth();
            int height = frame.GetHeight();
            int pitchY = frame.GetPitch(Plane.Y);
            int pitchUv = frame.GetPitch(Plane.U);
            int dstPitch = width*4;
            var dst = new byte[dstPitch*height];
            int offsetY = 0;
            int offsetUv = 0;

            int offset = dstPitch*(height - 1); // We start at last line

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int Y = srcY[offsetY + x] + matrix.OffsetY;
                    int U = srcU[offsetUv + x] - 128;
                    int V = srcV[offsetUv + x] - 128;
                    int b = ((matrix.Yb*Y + matrix.Ub*U + matrix.Vb*V + 4096) >> 13);
                    int g = ((matrix.Yg*Y + matrix.Ug*U + matrix.Vg*V + 4096) >> 13);
                    int r = ((matrix.Yr*Y + matrix.Ur*U + matrix.Vr*V + 4096) >> 13);
                    dst[offset + x*4 + 0] = ClipByte(b);
                    dst[offset + x*4 + 1] = ClipByte(g); 
                    dst[offset + x*4 + 2] = ClipByte(r);
                    dst[offset + x*4 + 3] = 255;
                }

                offset -= dstPitch;
                offsetY += pitchY;
                offsetUv += pitchUv;
            }
            return new ManagedVideoFrame(dst, width*4, height, dstPitch, Colorspace.BGR32);
        }

        struct ConversionMatrix
        {
            public short Yr;
            public short Yg;
            public short Yb;
            public short Ur;
            public short Ug;
            public short Ub;
            public short Vr;
            public short Vg;
            public short Vb;
             
            public int OffsetY;
        };

        //from avs core
        private static ConversionMatrix BuildMatrix(double kr, double kb, int sy, int suv, int oy, int shift)
        {
            double mulfac = 1 << shift;
            double kg = 1.0 - kr - kb;
            const int srgb = 255;

            return new ConversionMatrix
            {
                Yb = (short)(srgb*1.000*mulfac/sy + 0.5),
                Ub = (short)(srgb*(1 - kb)*mulfac/suv + 0.5),
                Vb = (short)(srgb*0.000*mulfac/suv + 0.5),
                Yg = (short)(srgb*1.000*mulfac/sy + 0.5),
                Ug = (short)(srgb*(kb - 1)*kb/kg*mulfac/suv + 0.5),
                Vg = (short)(srgb*(kr - 1)*kr/kg*mulfac/suv + 0.5),
                Yr = (short)(srgb*1.000*mulfac/sy + 0.5),
                Ur = (short)(srgb*0.000*mulfac/suv + 0.5),
                Vr = (short)(srgb*(1 - kr)*mulfac/suv + 0.5),
                OffsetY = -oy
            };
        }
    }
}