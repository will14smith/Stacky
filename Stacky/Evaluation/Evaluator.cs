using System.Collections.Immutable;
using Stacky.Parsing.Syntax;

namespace Stacky.Evaluation;

public class Evaluator
{
    private readonly SyntaxProgram _program;

    public Evaluator(SyntaxProgram program)
    {
        _program = program;
    }

    public ImmutableStack<EvaluationValue> Run() => Run(ImmutableStack<EvaluationValue>.Empty);
    public ImmutableStack<EvaluationValue> Run(ImmutableStack<EvaluationValue> initial)
    {
        var state = new EvaluationState(_program, initial);
        var function = state.GetFunction("main");

        state = RunFunction(state, function);

        return state.Stack;
    }
    
    private static EvaluationState RunFunction(EvaluationState state, SyntaxFunction function) => RunExpression(state, function.Body);

    internal static EvaluationState RunExpression(EvaluationState state, SyntaxExpression expr) =>
        expr switch
        {
            SyntaxExpression.LiteralInteger literal => state.Push(new EvaluationValue.Int64(literal.Value)),
            SyntaxExpression.LiteralString literal => state.Push(new EvaluationValue.String(literal.Value)),
            SyntaxExpression.Function function => state.Push(new EvaluationValue.Function(function.Body)),
            
            SyntaxExpression.Application application => RunApplication(state, application),
            SyntaxExpression.Identifier identifier => RunIdentifier(state, identifier),
            
            _ => throw new ArgumentOutOfRangeException(nameof(expr))
        };
    
    private static EvaluationState RunApplication(EvaluationState state, SyntaxExpression.Application application) => application.Expressions.Aggregate(state, RunExpression);

    private static EvaluationState RunIdentifier(EvaluationState state, SyntaxExpression.Identifier identifier)
    {
        if (EvaluationBuiltIn.Map.TryGetValue(identifier.Value, out var handler))
        {
            return handler(state);
        }
        
        var function = state.GetFunction(identifier.Value);
        return RunFunction(state, function);

    }
}