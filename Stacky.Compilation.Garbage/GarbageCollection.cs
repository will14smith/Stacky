namespace Stacky.Compilation.Garbage;

public static class GarbageCollection
{
    public static unsafe ContextRef New() => new (Interop.New());
    public static unsafe void Destroy(this ContextRef contextRef) => Interop.Destroy(contextRef.Context);

    public static unsafe DataRef Allocate(this ContextRef contextRef, AllocationType type) => new(Interop.Allocate(contextRef.Context, ref type));
    public static unsafe DataRef AllocateRaw(this ContextRef contextRef, ulong size) => new(Interop.AllocateRaw(contextRef.Context, size));
    
    public static unsafe void RootAdd(this ContextRef contextRef, DataRef data) => Interop.RootAdd(contextRef.Context, data.Pointer);
    public static unsafe void RootRemove(this ContextRef contextRef, DataRef data) => Interop.RootRemove(contextRef.Context, data.Pointer);
    
}