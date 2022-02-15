using Stacky.Compilation.LLVM;

namespace Stacky.Compilation;

public class CompilerAllocator
{
    private readonly CompilerEmitter _emitter;

    private readonly Lazy<CompilerValue> _gcAllocateRaw;
    private readonly Lazy<CompilerValue> _gcRootAdd;
    private readonly Lazy<CompilerValue> _gcRootRemove;
    
    public CompilerAllocator(CompilerEmitter emitter)
    {
        _emitter = emitter;
        
        _gcAllocateRaw = new Lazy<CompilerValue>(() => _emitter.DefineNativeFunction("gc_allocate_raw", _emitter.NativeFunctions.GcAllocateRaw));
        _gcRootAdd = new Lazy<CompilerValue>(() => _emitter.DefineNativeFunction("gc_root_add", _emitter.NativeFunctions.GcRootAdd));
        _gcRootRemove = new Lazy<CompilerValue>(() => _emitter.DefineNativeFunction("gc_root_remove", _emitter.NativeFunctions.GcRootRemove));
    }

    public CompilerValue Allocate(CompilerStruct type)
    {
        // TODO allocate type & header information
        return AllocateRaw(type.Type, _emitter.StructSize(type));
    }
    public CompilerValue AllocateRaw(CompilerType type, long length) => AllocateRaw(type, _emitter.Literal(length));
    public CompilerValue AllocateRaw(CompilerType type, CompilerValue length) => _emitter.Call(_gcAllocateRaw.Value, type, length);

    public void AddRoot(CompilerValue value)
    {
        if (IsApplicable(value))
        {
            // TODO cast value pointer to char*
            _emitter.CallVoid(_gcRootAdd.Value, value);
        }
    }
    
    public void RemoveRoot(CompilerValue value)
    {
        if (IsApplicable(value))
        {
            // TODO cast value pointer to char*
            _emitter.CallVoid(_gcRootRemove.Value, value);
        }
    }
    
    private static bool IsApplicable(CompilerValue value)
    {
        // TODO expand this???
        return value.Type is CompilerType.String or CompilerType.Struct;
    }
}