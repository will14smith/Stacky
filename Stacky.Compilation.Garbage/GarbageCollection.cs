using System.Runtime.InteropServices;
using Stacky.Compilation.Garbage.Types;

namespace Stacky.Compilation.Garbage;

internal class GarbageCollectionInterop
{
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "gc_new", ExactSpelling = true)]
    internal static extern IntPtr New();
    
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "gc_destroy", ExactSpelling = true)]
    internal static extern void Destroy(IntPtr context);
    
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "gc_allocate", ExactSpelling = true)]
    internal static extern IntPtr Allocate(IntPtr context, IntPtr type);
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "gc_allocate_raw", ExactSpelling = true)]
    internal static extern IntPtr AllocateRaw(IntPtr context, ulong size);
    
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "gc_root_add", ExactSpelling = true)]
    internal static extern void RootAdd(IntPtr context, IntPtr pointer);
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "gc_root_remove", ExactSpelling = true)]
    internal static extern void RootRemove(IntPtr context, IntPtr pointer);
    
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "gc_collect", ExactSpelling = true)]
    internal static extern void Collect(IntPtr context);
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "gc_stats", ExactSpelling = true)]
    internal static extern GarbageCollectionStats Stats(IntPtr context);
}

public static class GarbageCollection
{
    public static GarbageCollectionRef New() => new (GarbageCollectionInterop.New());
    public static void Destroy(this GarbageCollectionRef garbageCollectionRef) => GarbageCollectionInterop.Destroy(garbageCollectionRef.Context);

    public static DataRef Allocate(this GarbageCollectionRef garbageCollectionRef, AllocationType type)
    {
        // This probably leaks memory but...
        var typePointer = Marshal.AllocHGlobal(Marshal.SizeOf<AllocationType>());
        Marshal.StructureToPtr(type, typePointer, false);
        return new(GarbageCollectionInterop.Allocate(garbageCollectionRef.Context, typePointer));
    }

    public static DataRef AllocateRaw(this GarbageCollectionRef garbageCollectionRef, ulong size) => new(GarbageCollectionInterop.AllocateRaw(garbageCollectionRef.Context, size));
    
    public static void RootAdd(this GarbageCollectionRef garbageCollectionRef, DataRef data) => GarbageCollectionInterop.RootAdd(garbageCollectionRef.Context, data.Pointer);
    public static void RootRemove(this GarbageCollectionRef garbageCollectionRef, DataRef data) => GarbageCollectionInterop.RootRemove(garbageCollectionRef.Context, data.Pointer);

    public static void Collect(this GarbageCollectionRef garbageCollectionRef) => GarbageCollectionInterop.Collect(garbageCollectionRef.Context);
    public static GarbageCollectionStats Stats(this GarbageCollectionRef garbageCollectionRef) => GarbageCollectionInterop.Stats(garbageCollectionRef.Context);
}

public struct GarbageCollectionRef
{
    internal GarbageCollectionRef(IntPtr context)
    {
        Context = context;
    }

    internal IntPtr Context { get; }
}

public struct DataRef
{
    internal DataRef(IntPtr pointer)
    {
        Pointer = pointer;
    }

    internal IntPtr Pointer { get; }
}

[StructLayout(LayoutKind.Sequential)]
public struct GarbageCollectionStats
{
    public long AllocatedItems;
    public long AllocatedItemsSize;
    public long RootedItems;
    public long ReachableItems;

}
