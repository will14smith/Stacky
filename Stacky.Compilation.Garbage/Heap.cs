using System.Collections;
using System.Runtime.InteropServices;

namespace Stacky.Compilation.Garbage;

internal static class HeapInterop
{
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "heap_new", ExactSpelling = true)]
    internal static extern IntPtr New();
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "heap_destroy", ExactSpelling = true)]
    internal static extern void Destroy(IntPtr heap);
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "heap_count", ExactSpelling = true)]
    internal static extern long Count(IntPtr heap);
    
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "heap_add", ExactSpelling = true)]
    internal static extern void Add(IntPtr heap, IntPtr value);
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "heap_remove", ExactSpelling = true)]
    internal static extern void Remove(IntPtr heap, IntPtr value);
    
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "heap_iterate_init", ExactSpelling = true)]
    internal static extern void IterateInit(IntPtr heap, ref HeapIterator iterator);
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "heap_iterate_current", ExactSpelling = true)]
    internal static extern IntPtr IterateCurrent(ref HeapIterator iterator);
    [DllImport("gc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "heap_iterate_next", ExactSpelling = true)]
    internal static extern int IterateNext(ref HeapIterator iterator);
}

public static class Heap
{
    public static HeapRef New() => new (HeapInterop.New());
    public static void Destroy(this HeapRef heapRef) => HeapInterop.Destroy(heapRef.Heap);
    public static long Count(this HeapRef heapRef) => HeapInterop.Count(heapRef.Heap);
    
    public static void Add(this HeapRef heapRef, IntPtr value) => HeapInterop.Add(heapRef.Heap, value);
    public static void Remove(this HeapRef heapRef, IntPtr value) => HeapInterop.Remove(heapRef.Heap, value);

    public static IEnumerator<IntPtr> GetEnumerator(this HeapRef heapRef)
    {
        var iterator = new HeapIterator();
        HeapInterop.IterateInit(heapRef.Heap, ref iterator);
        return new HeapEnumerator(iterator);
    }

    public static IntPtr Current(this ref HeapIterator iterator) => HeapInterop.IterateCurrent(ref iterator);
    public static bool Next(this ref HeapIterator iterator) => HeapInterop.IterateNext(ref iterator) != 0;
}

public class HeapEnumerator : IEnumerator<IntPtr>
{
    private HeapIterator _iterator;

    public HeapEnumerator(HeapIterator iterator)
    {
        _iterator = iterator;
    }

    public bool MoveNext() => _iterator.Next();
    public IntPtr Current => _iterator.Current();

    public void Reset() => throw new NotSupportedException();
    object IEnumerator.Current => Current;
    public void Dispose() { }
}

public struct HeapRef : IEnumerable<IntPtr>
{
    internal HeapRef(IntPtr heap)
    {
        Heap = heap;
    }

    internal IntPtr Heap { get; }

    public long Count => this.Count();
    
    public IEnumerator<IntPtr> GetEnumerator() => Garbage.Heap.GetEnumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public struct HeapIterator
{
    internal IntPtr Heap;
    internal long Index;
}