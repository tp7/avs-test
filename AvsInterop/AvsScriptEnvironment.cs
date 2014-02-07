using System;
using AvsCommon.Exceptions;

namespace AvsInterop
{
    internal class AvsScriptEnvironment : IDisposable
    {
        private readonly IntPtr _env;
        private readonly AvsWrapper _avsApi;

        internal AvsScriptEnvironment(string dllPath)
        {
            _avsApi = new AvsWrapper(dllPath);
            _env = _avsApi.AvsCreateScriptEnvironment();
            if (_env == IntPtr.Zero)
            {
                throw new AvsOperationException("Couldn't create script environment");
            }
        }

        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        ~AvsScriptEnvironment()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            _avsApi.AvsDeleteScriptEnvironment(_env);
            _avsApi.Dispose();
        }

        internal AvsValue Invoke(string name, AvsValue arguments, bool disposeArguments = false)
        {
            try
            {
                return new AvsValue(_avsApi.AvsInvoke(_env, name, arguments.GetAvsValue(), IntPtr.Zero), this);
            }
            finally
            {
                if (disposeArguments)
                {
                    arguments.Dispose();
                }
            }
        }

        internal IntPtr AvsGetFrame(IntPtr clip, int n)
        {
            return _avsApi.AvsGetFrame(clip, n);
        }

        internal AvsClip AvsTakeClip(AvsValueStructure value)
        {
            return new AvsClip(_avsApi.AvsTakeClip(value, _env), this);
        }

        internal void AvsReleaseVideoFrame(IntPtr frame)
        {
            _avsApi.AvsReleaseVideoFrame(frame);
        }

        internal void AvsReleaseClip(IntPtr clip)
        {
            _avsApi.AvsReleaseClip(clip);
        }

        internal IntPtr AvsGetVideoInfo(IntPtr clip)
        {
            return _avsApi.AvsGetVideoInfo(clip);
        }
    }
}