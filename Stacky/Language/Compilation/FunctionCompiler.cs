using Stacky.Language.Compilation.LLVM;
using Stacky.Language.Syntax;

namespace Stacky.Language.Compilation;

public class FunctionCompiler
{
    private readonly SyntaxFunction _function;
    private readonly CompilerAllocator _allocator;
    private readonly CompilerEnvironment _environment;
    private readonly CompilerEmitter _emitter;
    private readonly CompilerIntrinsics _intrinsics;

    public FunctionCompiler(SyntaxFunction function, CompilerAllocator allocator, CompilerEnvironment environment, CompilerEmitter emitter, CompilerIntrinsics intrinsics)
    {
        _function = function;

        _allocator = allocator;
        _environment = environment;
        _emitter = emitter;
        _intrinsics = intrinsics;
    }

    public void Compile()
    {
        var stack = new CompilerStack(_allocator, _emitter);
        var definition = _environment.GetFunction(_function.Name.Value);
        var type = (CompilerType.Function) definition.Type;
        
        foreach (var input in type.Inputs) { stack = stack.PushType(input); }

        var compiler = new ExpressionCompiler(_emitter, _environment, _intrinsics);
        
        _emitter.BeginBlock(definition);
        stack = compiler.Compile(stack, _function.Body);

        _emitter.RetVoid();
        _emitter.VerifyFunction(definition);
    }
}