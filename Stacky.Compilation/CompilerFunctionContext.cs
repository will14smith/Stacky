using Stacky.Compilation.LLVM;

namespace Stacky.Compilation;

public class CompilerFunctionContext
{
    public CompilerAllocator Allocator { get; }
    public CompilerEmitter Emitter { get; }

    public CompilerFunctionContext(CompilerAllocator allocator, CompilerEmitter emitter)
    {
        Allocator = allocator;
        Emitter = emitter;
    }
}