using Stacky.Compilation.LLVM;

namespace Stacky.Compilation;

public class CompilerAllocator
{
    private readonly CompilerEmitter _emitter;
    
    public CompilerAllocator(CompilerEmitter emitter)
    {
        _emitter = emitter;
    }

    public CompilerValue Allocate(CompilerStruct type) => _emitter.GC.Allocate(type);
    public CompilerValue AllocateRaw(CompilerType type, long length) => AllocateRaw(type, _emitter.Literal(length));
    public CompilerValue AllocateRaw(CompilerType type, CompilerValue length) => _emitter.GC.AllocateRaw(type, length);

    public void AddRoot(CompilerValue value)
    {
        if (IsApplicable(value))
        {
            _emitter.GC.RootAdd(value);
        }
    }
    
    public void RemoveRoot(CompilerValue value)
    {
        if (IsApplicable(value))
        {
            _emitter.GC.RootRemove(value);
        }
    }
    
    private static bool IsApplicable(CompilerValue value) => value.Type is CompilerType.String or CompilerType.Struct or CompilerType.Pointer;
}