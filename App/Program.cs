using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace App
{
    [ComVisible(true)]
    [Guid("ed9e0067-f920-4b8a-aec1-071dc6a826ac")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInvoker
    {
        void InvokeWithObject(object arg);
        void InvokeWithInt(int arg);
    }

    [ClassInterface(ClassInterfaceType.None)]
    public class ManagedInvoker : IInvoker
    {
        void IInvoker.InvokeWithObject(object arg) { }
        void IInvoker.InvokeWithInt(int arg) { }
    }

    [SuppressUnmanagedCodeSecurity]
    unsafe static class NativeLib
    {
        const string Path = @"..\..\..\x64\Release\NativeLib.dll";
        [DllImport(Path)]
        public static extern void SetInvoker(void* ptr);

        [DllImport(Path)]
        public static extern int InvokeWithObject(object arg);

        [DllImport(Path)]
        public static extern int InvokeWithInt(int arg);

        [DllImport(Path)]
        public static extern void SetInvokerObject(void* fptr, void* obj);

        [DllImport(Path)]
        public static extern int InvokeWithIntFast(int arg);
    }


    unsafe class Program
    {
        delegate void Callback_t(IntPtr obj, int arg);
        static Callback_t CallbackInstance;

        static void Callback(IntPtr obj, int arg)
        {
            var handle = GCHandle.FromIntPtr(obj);
            ((IInvoker)handle.Target).InvokeWithInt(arg);
        }

        static void Main(string[] args)
        {
            var mInvoker = new ManagedInvoker();
            var pUnk = Marshal.GetIUnknownForObject(mInvoker);

            var interop = new InteropInvoker();
            interop.SetInvoker(pUnk.ToPointer());
            NativeLib.SetInvoker(pUnk.ToPointer());

            // Instead of marshalling or projecting an interface over an object
            // pass a handle to the object and unwrap it on demand. This does place
            // some additional pressure on the GC when done excessively, but a single
            // instance is practically noise if it is only one object.
            //
            // The delegate must be stored somewhere or else the GC will collected it.
            // This is because the returned function pointer doesn't count as a
            // reference.
            CallbackInstance = new Callback_t(Callback);
            var fptr = Marshal.GetFunctionPointerForDelegate(CallbackInstance);

            // Remember to free the handle or the GC will never collect the object.
            GCHandle handle = GCHandle.Alloc(mInvoker);
            NativeLib.SetInvokerObject(fptr.ToPointer(), GCHandle.ToIntPtr(handle).ToPointer());

            // Warm up process
            for (int i = 0; i < 100; ++i)
            {
                interop.InvokeWithInt(i);
                interop.InvokeWithObject(i);
                NativeLib.InvokeWithInt(i);
                NativeLib.InvokeWithObject(i);
                NativeLib.InvokeWithIntFast(i);
            }

            // Measure
            int iterations = 10_000_000;
            {
                var sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < iterations; ++i)
                {
                    interop.InvokeWithInt(i);
                }
                sw.Stop();
                Console.WriteLine($"{nameof(InteropInvoker)}.{nameof(InteropInvoker.InvokeWithInt)} - {sw.ElapsedMilliseconds}");
            }
            {
                var sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < iterations; ++i)
                {
                    interop.InvokeWithObject(i);
                }
                sw.Stop();
                Console.WriteLine($"{nameof(InteropInvoker)}.{nameof(InteropInvoker.InvokeWithObject)} - {sw.ElapsedMilliseconds}");
            }
            {
                var sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < iterations; ++i)
                {
                    NativeLib.InvokeWithInt(i);
                }
                sw.Stop();
                Console.WriteLine($"{nameof(NativeLib)}.{nameof(NativeLib.InvokeWithInt)} - {sw.ElapsedMilliseconds}");
            }
            {
                var sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < iterations; ++i)
                {
                    NativeLib.InvokeWithObject(i);
                }
                sw.Stop();
                Console.WriteLine($"{nameof(NativeLib)}.{nameof(NativeLib.InvokeWithObject)} - {sw.ElapsedMilliseconds}");
            }
            {
                var sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < iterations; ++i)
                {
                    NativeLib.InvokeWithIntFast(i);
                }
                sw.Stop();
                Console.WriteLine($"{nameof(NativeLib)}.{nameof(NativeLib.InvokeWithIntFast)} - {sw.ElapsedMilliseconds}");
            }
        }
    }
}
