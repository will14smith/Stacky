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

public static class CompilerFunctionContextExtensions
{
    public static CompilerStack Invoke(this CompilerFunctionContext context, CompilerStack stack, CompilerValue closure)
    {
        var function = context.Emitter.Load(context.Emitter.FieldPointer(closure, ClosureCompiler.FunctionField));
        var state = context.Emitter.Load(context.Emitter.FieldPointer(closure, ClosureCompiler.StateField));

        stack = stack.Push(state);

        return ExpressionCompiler.CallFunction(context.Emitter, stack, function);
    }
}