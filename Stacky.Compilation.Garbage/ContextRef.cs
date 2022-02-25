namespace Stacky.Compilation.Garbage;

public struct ContextRef
{
    internal unsafe ContextRef(Context* context)
    {
        Context = context;
    }

    internal unsafe Context* Context { get; }
}