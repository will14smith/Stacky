using Stacky.Compilation.LLVM;

namespace Stacky.Compilation;

public class CompilerAllocator
{
    private readonly CompilerEmitter _emitter;
    
    public CompilerAllocator(CompilerEmitter emitter)
    {
        _emitter = emitter;
    }

    public CompilerValue Allocate(CompilerStruct type)
    {
        // var value = AllocateRaw(type.Type, _emitter.StructSize(type));
        // return _emitter.StructCast(value, type);
        
        return _emitter.GC.Allocate(type);
    }
    public CompilerValue AllocateRaw(CompilerType type, long length) => AllocateRaw(type, _emitter.Literal(length));
    public CompilerValue AllocateRaw(CompilerType type, CompilerValue length)
    {
        // return _emitter.Call(_gcAllocateRaw.Value, type, length);

        return _emitter.GC.AllocateRaw(type, length);
    }

    public void AddRoot(CompilerValue value)
    {
        if (IsApplicable(value))
        {
            // TODO cast value pointer to char*
            // _emitter.CallVoid(_gcRootAdd.Value, value);
            
            _emitter.GC.RootAdd(value);
        }
    }
    
    public void RemoveRoot(CompilerValue value)
    {
        if (IsApplicable(value))
        {
            // TODO cast value pointer to char*
            // _emitter.CallVoid(_gcRootRemove.Value, value);
            
            _emitter.GC.RootRemove(value);
        }
    }
    
    private static bool IsApplicable(CompilerValue value)
    {
        // TODO expand this???
        return value.Type is CompilerType.String or CompilerType.Struct;
    }
}