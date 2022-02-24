using Stacky.Compilation.LLVM;
using Stacky.Parsing.Typing;

namespace Stacky.Compilation;

public class FunctionCompiler
{
    private readonly TypedFunction _function;
    private readonly CompilerAllocator _allocator;
    private readonly CompilerEnvironment _environment;
    private readonly CompilerEmitter _emitter;
    private readonly CompilerIntrinsicRegistry _intrinsics;
    private readonly ClosureCompiler _closures;

    public FunctionCompiler(TypedFunction function, CompilerAllocator allocator, CompilerEnvironment environment, CompilerEmitter emitter, CompilerIntrinsicRegistry intrinsics, CompilerTypeBuilder typeBuilder)
    {
        _function = function;

        _allocator = allocator;
        _environment = environment;
        _emitter = emitter;
        _intrinsics = intrinsics;

        _closures = new ClosureCompiler(function, allocator, environment, emitter, intrinsics, typeBuilder);
    }

    public void Compile()
    {
        var closures = _closures.Compile();

        var definition = _environment.GetFunction(_function.Name.Value);
        CompileFunction(definition, _function.Body, closures);
    }

    private void CompileFunction(CompilerValue definition, TypedExpression body, IReadOnlyDictionary<TypedExpression.Closure, CompilerClosure> closures)
    {
        var stack = new CompilerStack(_allocator, _emitter);
        var type = (CompilerType.Function)definition.Type;
        
        foreach (var input in type.Inputs)
        {
            stack = stack.PushType(input);
        }

        var compiler = new ExpressionCompiler(_allocator, _emitter, _environment, _intrinsics, closures);

        var entry = _emitter.CreateBlock(definition, "entry");
        _emitter.BeginBlock(entry);
        
        stack = compiler.Compile(stack, body);

        if (_function.Name.Value == "main" && ReferenceEquals(body, _function.Body))
        {
            // TODO remove downcast...
            _emitter.Ret(_emitter.Truncate(_emitter.Literal(0), new CompilerType.Int()));
        }
        else
        {
            _emitter.RetVoid();
        }

        _emitter.VerifyFunction(definition);
    } 
}