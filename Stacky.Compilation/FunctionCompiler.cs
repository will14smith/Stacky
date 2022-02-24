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
    private readonly CompilerTypeBuilder _typeBuilder;

    public FunctionCompiler(TypedFunction function, CompilerAllocator allocator, CompilerEnvironment environment, CompilerEmitter emitter, CompilerIntrinsicRegistry intrinsics, CompilerTypeBuilder typeBuilder)
    {
        _function = function;

        _allocator = allocator;
        _environment = environment;
        _emitter = emitter;
        _intrinsics = intrinsics;
        _typeBuilder = typeBuilder;
    }

    public void Compile()
    {
        var closures = CompileClosures(_function.Body);

        var definition = _environment.GetFunction(_function.Name.Value);
        CompileFunction(definition, _function.Body, closures);
    }

    private void CompileFunction(CompilerValue definition, TypedExpression body, IReadOnlyDictionary<TypedExpression.Closure, CompilerValue> closures)
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

    private IReadOnlyDictionary<TypedExpression.Closure, CompilerValue> CompileClosures(TypedExpression expression)
    {
        var mapping = new Dictionary<TypedExpression.Closure, CompilerValue>();

        var functions = ExtractClosures(expression);

        var i = 0;
        foreach (var function in functions)
        {
            var type = _typeBuilder.BuildFunction((StackyType.Function)function.Type.LastOutput());
            
            var definition = _environment.DefineFunction($"__{_function.Name.Value}_anon<{i++}>", type);
            CompileFunction(definition, function.Body, mapping);
            
            mapping.Add(function, definition);
        }
        
        return mapping;
    }

    private static IEnumerable<TypedExpression.Closure> ExtractClosures(TypedExpression expression)
    {
        return expression switch
        {
            TypedExpression.LiteralInteger => Array.Empty<TypedExpression.Closure>(),
            TypedExpression.LiteralString => Array.Empty<TypedExpression.Closure>(),
            TypedExpression.Identifier => Array.Empty<TypedExpression.Closure>(),

            TypedExpression.Closure closure => ExtractClosures(closure.Body).Append(closure),
            TypedExpression.Application application => application.Expressions.SelectMany(ExtractClosures),
            TypedExpression.Binding binding => ExtractClosures(binding.Body),

            _ => throw new ArgumentOutOfRangeException(nameof(expression))
        };
    }
}