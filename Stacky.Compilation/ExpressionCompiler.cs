using Stacky.Compilation.LLVM;
using Stacky.Parsing.Syntax;

namespace Stacky.Compilation;

public class ExpressionCompiler
{
    private readonly CompilerEmitter _emitter;
    private readonly CompilerEnvironment _environment;
    private readonly CompilerIntrinsics _intrinsics;

    public ExpressionCompiler(CompilerEmitter emitter, CompilerEnvironment environment, CompilerIntrinsics intrinsics)
    {
        _emitter = emitter;
        _environment = environment;
        _intrinsics = intrinsics;
    }

    public CompilerStack Compile(CompilerStack stack, SyntaxExpression expression)
    {
        return expression switch
        {
            SyntaxExpression.LiteralInteger literalInteger => CompileLiteral(stack, literalInteger),
            SyntaxExpression.LiteralString literalString => CompileLiteral(stack, literalString),

            SyntaxExpression.Application application => CompileApplication(stack, application),
            SyntaxExpression.Identifier identifier => CompileIdentifier(stack, identifier),
            SyntaxExpression.Conditional conditional => throw new NotImplementedException(),

            _ => throw new ArgumentOutOfRangeException(nameof(expression))
        };
    }
    
    private CompilerStack CompileLiteral(CompilerStack stack, SyntaxExpression.LiteralInteger literal) => stack.Push(_emitter.Literal(literal.Value));
    private CompilerStack CompileLiteral(CompilerStack stack, SyntaxExpression.LiteralString literal) => stack.Push(_emitter.Literal(literal.Value));

    private CompilerStack CompileApplication(CompilerStack stack, SyntaxExpression.Application application) => application.Expressions.Aggregate(stack, Compile);

    private CompilerStack CompileIdentifier(CompilerStack stack, SyntaxExpression.Identifier identifier)
    {
        if (_intrinsics.TryCompile(identifier.Value, ref stack))
        {
            return stack;
        }

        var function = _environment.GetFunction(identifier.Value);
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

        _emitter.CallVoid(function);
        
        foreach (var outputType in functionType.Outputs)
        {
            stack = stack.PushType(outputType);
        }

        return stack;
    }
}