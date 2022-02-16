using Stacky.Compilation.LLVM;
using Stacky.Parsing.Typing;

namespace Stacky.Compilation;

public partial class ExpressionCompiler
{
    private readonly CompilerAllocator _allocator;
    private readonly CompilerEmitter _emitter;
    private readonly CompilerEnvironment _environment;
    private readonly CompilerIntrinsicRegistry _intrinsics;
    private readonly IReadOnlyDictionary<TypedExpression.Function, CompilerValue> _anonymousFunctionMapping;

    public ExpressionCompiler(CompilerAllocator allocator, CompilerEmitter emitter, CompilerEnvironment environment, CompilerIntrinsicRegistry intrinsics, IReadOnlyDictionary<TypedExpression.Function, CompilerValue> anonymousFunctionMapping)
    {
        _allocator = allocator;
        _emitter = emitter;
        _environment = environment;
        _intrinsics = intrinsics;
        _anonymousFunctionMapping = anonymousFunctionMapping;
    }

    public CompilerStack Compile(CompilerStack stack, TypedExpression expression)
    {
        return expression switch
        {
            TypedExpression.LiteralInteger literalInteger => CompileLiteral(stack, literalInteger),
            TypedExpression.LiteralString literalString => CompileLiteral(stack, literalString),

            TypedExpression.Function function => CompileFunction(stack, function),
            
            TypedExpression.Application application => CompileApplication(stack, application),
            TypedExpression.Identifier identifier => CompileIdentifier(stack, identifier),

            _ => throw new ArgumentOutOfRangeException(nameof(expression))
        };
    }
    
    private CompilerStack CompileLiteral(CompilerStack stack, TypedExpression.LiteralInteger literal) => stack.Push(_emitter.Literal(literal));
    private CompilerStack CompileLiteral(CompilerStack stack, TypedExpression.LiteralString literal) => stack.Push(_emitter.Literal(literal.Value));

    private CompilerStack CompileFunction(CompilerStack stack, TypedExpression.Function function) => stack.Push(_anonymousFunctionMapping[function]);

    private CompilerStack CompileApplication(CompilerStack stack, TypedExpression.Application application) => application.Expressions.Aggregate(stack, Compile);

    private CompilerStack CompileIdentifier(CompilerStack stack, TypedExpression.Identifier identifier)
    {
        if (identifier.Value.Length > 1)
        {
            switch (identifier.Value[0])
            {
                case '@': return CompileInit(stack, identifier);                
                case '#': return CompileGetter(stack, identifier);                
                case '~': return CompileSetter(stack, identifier);                
            }
        }

        if (_intrinsics.TryCompile(identifier.Value, ref stack))
        {
            return stack;
        }

        var function = _environment.GetFunction(identifier.Value);
        return CallFunction(_emitter, stack, function);
    }

    public static CompilerStack CallFunction(CompilerEmitter emitter, CompilerStack stack, CompilerValue function)
    {
        var functionType = (CompilerType.Function)function.Type;

        for (var i = functionType.Inputs.Count - 1; i >= 0; i--)
        {
            stack = stack.PopType(out var stackType);
            var inputType = functionType.Inputs[i];
            if (stackType != inputType)
            {
                throw new InvalidCastException($"unable to cast '{stackType}' to '{inputType}'");
            }
        }

        emitter.CallVoid(function);

        foreach (var outputType in functionType.Outputs)
        {
            stack = stack.PushType(outputType);
        }

        return stack;
    }
}