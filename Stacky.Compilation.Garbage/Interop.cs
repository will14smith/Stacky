using System.Runtime.InteropServices;

namespace Stacky.Compilation.Garbage;

internal class Interop
{
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "gc_new", ExactSpelling = true)]
    internal static extern unsafe Context* New();
    
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "gc_destroy", ExactSpelling = true)]
    internal static extern unsafe void Destroy(Context* context);
    
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "gc_allocate", ExactSpelling = true)]
    internal static extern unsafe void* Allocate(Context* context, ref AllocationType type);
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "gc_allocate_raw", ExactSpelling = true)]
    internal static extern unsafe void* AllocateRaw(Context* context, ulong size);

    
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "gc_root_add", ExactSpelling = true)]
    internal static extern unsafe void RootAdd(Context* context, void* pointer);
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "gc_root_remove", ExactSpelling = true)]
    internal static extern unsafe void RootRemove(Context* context, void* pointer);

}