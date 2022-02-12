using LLVMSharp;
using Stacky.Compilation.LLVM;
using Stacky.Parsing.Syntax;

namespace Stacky.Compilation;

public class ExpressionCompiler
{
    private readonly CompilerEmitter _emitter;
    private readonly CompilerEnvironment _environment;
    private readonly CompilerIntrinsics _intrinsics;
    private readonly IReadOnlyDictionary<SyntaxExpression.Function, CompilerValue> _anonymousFunctionMapping;

    public ExpressionCompiler(CompilerEmitter emitter, CompilerEnvironment environment, CompilerIntrinsics intrinsics, IReadOnlyDictionary<SyntaxExpression.Function, CompilerValue> anonymousFunctionMapping)
    {
        _emitter = emitter;
        _environment = environment;
        _intrinsics = intrinsics;
        _anonymousFunctionMapping = anonymousFunctionMapping;
    }

    public CompilerStack Compile(CompilerStack stack, SyntaxExpression expression)
    {
        return expression switch
        {
            SyntaxExpression.LiteralInteger literalInteger => CompileLiteral(stack, literalInteger),
            SyntaxExpression.LiteralString literalString => CompileLiteral(stack, literalString),

            SyntaxExpression.Function function => CompileFunction(stack, function),
            
            SyntaxExpression.Application application => CompileApplication(stack, application),
            SyntaxExpression.Identifier identifier => CompileIdentifier(stack, identifier),

            _ => throw new ArgumentOutOfRangeException(nameof(expression))
        };
    }
    
    private CompilerStack CompileLiteral(CompilerStack stack, SyntaxExpression.LiteralInteger literal) => stack.Push(_emitter.Literal(literal.Value));
    private CompilerStack CompileLiteral(CompilerStack stack, SyntaxExpression.LiteralString literal) => stack.Push(_emitter.Literal(literal.Value));

    private CompilerStack CompileFunction(CompilerStack stack, SyntaxExpression.Function function) => stack.Push(_anonymousFunctionMapping[function]);

    private CompilerStack CompileApplication(CompilerStack stack, SyntaxExpression.Application application) => application.Expressions.Aggregate(stack, Compile);

    private CompilerStack CompileIdentifier(CompilerStack stack, SyntaxExpression.Identifier identifier)
    {
        if (_intrinsics.TryCompile(identifier.Value, ref stack))
        {
            return stack;
        }

        var function = _environment.GetFunction(identifier.Value);
        return CallFunction(_emitter, stack, function);
    }

    internal static CompilerStack CallFunction(CompilerEmitter emitter, CompilerStack stack, CompilerValue function)
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