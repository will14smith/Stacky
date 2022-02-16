using Stacky.Compilation.LLVM;

namespace Stacky.Compilation;

public class CompilerIntrinsicRegistry
{
    private readonly CompilerAllocator _allocator;
    private readonly CompilerEmitter _emitter;
    
    private readonly Dictionary<string, ICompilerIntrinsic> _map = new();

    public CompilerIntrinsicRegistry(CompilerAllocator allocator, CompilerEmitter emitter)
    {
        _allocator = allocator;
        _emitter = emitter;
    }

    public void Register(string name, ICompilerIntrinsic intrinsic)
    {
        _map.Add(name, intrinsic);
    }

    public bool TryCompile(string name, ref CompilerStack stack)
    {
        if (!_map.TryGetValue(name, out var handler))
        {
            return false;
        }
        
        stack = handler.Compile(new CompilerFunctionContext(_allocator, _emitter), stack);
        return true;
    }
}