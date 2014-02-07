using System;
using System.Runtime.InteropServices;
using AvsCommon;

namespace AvsInterop
{
    internal class AvsClip : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private class AvsVideoInfo
        {
            internal int width; // width=0 means no video
            internal int height; 
            internal uint fps_numerator;
            internal uint fps_denominator;
            internal int num_frames;

            internal int pixel_type;

            internal int audio_samples_per_second; // 0 means no audio
            internal int sample_type;
            internal long num_audio_samples;
            internal int nchannels;

            // Imagetype properties

            private int image_type;
        }

        private readonly IntPtr _clipPtr;
        private readonly AvsScriptEnvironment _env;
        private AvsVideoInfo _vi;

        internal AvsClip(IntPtr ptr, AvsScriptEnvironment env)
        {
            _clipPtr = ptr;
            _env = env;
        }

        internal AvsVideoFrame GetFrame(int n)
        {
            return new AvsVideoFrame(_env.AvsGetFrame(_clipPtr, n), Colorspace, _env);
        }

        public void Dispose()
        {
            _env.AvsReleaseClip(_clipPtr);
            GC.SuppressFinalize(this);
        }

        ~AvsClip()
        {
            _env.AvsReleaseClip(_clipPtr);
        }

        public int Width { get { return Vi.width; } }
        public int Height { get { return Vi.height; } }
        public int FrameCount { get { return Vi.num_frames; } }
        public Colorspace Colorspace { get { return (Colorspace)Vi.pixel_type; } }
        private bool HasAudio { get { return Vi.audio_samples_per_second != 0; } }

        private AvsVideoInfo Vi
        {
            get
            {
                if (_vi ==  null)
                {
                    var ptr = _env.AvsGetVideoInfo(_clipPtr);
                    _vi = (AvsVideoInfo)Marshal.PtrToStructure(ptr, typeof (AvsVideoInfo));
                }
                return _vi;
            }
        }
    }
}