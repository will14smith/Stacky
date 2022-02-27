using System.Collections;
using System.Runtime.InteropServices;

namespace Stacky.Compilation.Garbage;

internal static class RootInterop
{
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "root_new", ExactSpelling = true)]
    internal static extern IntPtr New();
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "root_destroy", ExactSpelling = true)]
    internal static extern void Destroy(IntPtr root);
    
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "root_add", ExactSpelling = true)]
    internal static extern void Add(IntPtr root, IntPtr value);
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "root_remove", ExactSpelling = true)]
    internal static extern void Remove(IntPtr root, IntPtr value);
    
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "root_iterate_init", ExactSpelling = true)]
    internal static extern void IterateInit(IntPtr root, ref RootIterator iterator);
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "root_iterate_current", ExactSpelling = true)]
    internal static extern IntPtr IterateCurrent(ref RootIterator iterator);
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "root_iterate_next", ExactSpelling = true)]
    internal static extern int IterateNext(ref RootIterator iterator);
}

public static class Root
{
    public static RootRef New() => new (RootInterop.New());
    public static void Destroy(this RootRef rootRef) => RootInterop.Destroy(rootRef.Root);
    
    public static void Add(this RootRef rootRef, IntPtr value) => RootInterop.Add(rootRef.Root, value);
    public static void Remove(this RootRef rootRef, IntPtr value) => RootInterop.Remove(rootRef.Root, value);

    public static IEnumerator<IntPtr> GetEnumerator(this RootRef rootRef)
    {
        var iterator = new RootIterator();
        RootInterop.IterateInit(rootRef.Root, ref iterator);
        return new RootEnumerator(iterator);
    }

    public static IntPtr Current(this ref RootIterator iterator) => RootInterop.IterateCurrent(ref iterator);
    public static bool Next(this ref RootIterator iterator) => RootInterop.IterateNext(ref iterator) != 0;
}

public class RootEnumerator : IEnumerator<IntPtr>
{
    private RootIterator _iterator;

    public RootEnumerator(RootIterator iterator)
    {
        _iterator = iterator;
    }

    public bool MoveNext() => _iterator.Next();
    public IntPtr Current => _iterator.Current();

    public void Reset() => throw new NotSupportedException();
    object IEnumerator.Current => Current;
    public void Dispose() { }
}

public struct RootRef : IEnumerable<IntPtr>
{
    internal RootRef(IntPtr root)
    {
        Root = root;
    }

    internal IntPtr Root { get; }
    
    public IEnumerator<IntPtr> GetEnumerator() => Garbage.Root.GetEnumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public struct RootIterator
{
    internal IntPtr Root;
    internal long Index;
    internal IntPtr Entry;
}