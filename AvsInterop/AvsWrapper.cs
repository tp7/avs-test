using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AvsCommon.Exceptions;

namespace AvsInterop
{
    internal class AvsWrapper : IDisposable
    {
        private const int AvsApiVersion = 3;
        private readonly IntPtr _library;
        private readonly Dictionary<string, Delegate> _functionsCache;

        internal AvsWrapper(string dllPath)
        {
            _library = WinApi.LoadLibrary(dllPath);
            if (_library == IntPtr.Zero)
            {
                throw new AvsLoadingException(Marshal.GetLastWin32Error(), dllPath);
            }
            _functionsCache = new Dictionary<string, Delegate>();
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        private delegate IntPtr AvsCreateScriptEnvironmentDelegate(int version);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate void AvsDeleteScriptEnvironmentDelegate(IntPtr env);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate AvsValueStructure AvsInvokeDelegate(IntPtr env, string name, AvsValueStructure args,
            IntPtr argNames);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr AvsTakeClipDelegate(AvsValueStructure value, IntPtr env);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate void AvsReleaseClipDelegate(IntPtr clip);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr AvsGetFrameDelegate(IntPtr clip, int n);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate void AvsReleaseVideoFrameDelegate(IntPtr frame);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr AvsGetVideoInfoDelegate(IntPtr clip);

        internal IntPtr AvsCreateScriptEnvironment()
        {
            return GetFunction<AvsCreateScriptEnvironmentDelegate>("avs_create_script_environment")(AvsApiVersion);
        }

        internal void AvsDeleteScriptEnvironment(IntPtr env)
        {
            GetFunction<AvsDeleteScriptEnvironmentDelegate>("avs_delete_script_environment")(env);
        }

        internal AvsValueStructure AvsInvoke(IntPtr env, string name, AvsValueStructure args,
            IntPtr argNames)
        {
            return GetFunction<AvsInvokeDelegate>("avs_invoke")(env, name, args, argNames);
        }

        internal IntPtr AvsTakeClip(AvsValueStructure value, IntPtr env)
        {
            return GetFunction<AvsTakeClipDelegate>("avs_take_clip")(value, env);
        }

        internal void AvsReleaseClip(IntPtr clip)
        {
            GetFunction<AvsReleaseClipDelegate>("avs_release_clip")(clip);
        }

        internal IntPtr AvsGetFrame(IntPtr clip, int n)
        {
            return GetFunction<AvsGetFrameDelegate>("avs_get_frame")(clip, n);
        }

        internal void AvsReleaseVideoFrame(IntPtr frame)
        {
            GetFunction<AvsReleaseVideoFrameDelegate>("avs_release_video_frame")(frame);
        }

        internal IntPtr AvsGetVideoInfo(IntPtr clip)
        {
            return GetFunction<AvsGetVideoInfoDelegate>("avs_get_video_info")(clip);
        }

        public void Dispose()
        {
            WinApi.FreeLibrary(_library);
        }

        private T GetFunction<T>(string name) where T : class
        {
            Delegate func;
            if (_functionsCache.TryGetValue(name, out func))
            {
                return func as T;
            }
            var ptr = WinApi.GetProcAddress(_library, name);
            if (ptr == IntPtr.Zero)
            {
                throw new AvsDispatchException(Marshal.GetLastWin32Error(), name);
            }
            func = Marshal.GetDelegateForFunctionPointer(ptr,typeof(T));
            _functionsCache.Add(name, func);
            return func as T;
        }
    }
}