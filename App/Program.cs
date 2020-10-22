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
    }

    unsafe class Program
    {
        static void Main(string[] args)
        {
            var mInvoker = new ManagedInvoker();
            var pUnk = Marshal.GetIUnknownForObject(mInvoker);

            var interop = new InteropInvoker();
            interop.SetInvoker(pUnk.ToPointer());
            NativeLib.SetInvoker(pUnk.ToPointer());

            // Warm up process
            for (int i = 0; i < 100; ++i)
            {
                interop.InvokeWithInt(i);
                interop.InvokeWithObject(i);
                NativeLib.InvokeWithInt(i);
                NativeLib.InvokeWithObject(i);
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
        }
    }
}
