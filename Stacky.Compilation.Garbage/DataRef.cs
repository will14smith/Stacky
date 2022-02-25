namespace Stacky.Compilation.Garbage;

public struct DataRef
{
    internal unsafe DataRef(void* pointer)
    {
        Pointer = pointer;
    }

    internal unsafe void* Pointer { get; }
}