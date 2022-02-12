using Stacky.Parsing.Syntax;

namespace Stacky.Evaluation;

public class Evaluator
{
    private readonly SyntaxProgram _program;

    public Evaluator(SyntaxProgram program)
    {
        _program = program;
    }

    public void Run()
    {
        var state = new EvaluationState(_program);
        var function = state.GetFunction("main");

        RunFunction(state, function);
    }
    
    private static EvaluationState RunFunction(EvaluationState state, SyntaxFunction function) => RunExpression(state, function.Body);

    private static EvaluationState RunExpression(EvaluationState state, SyntaxExpression expr) =>
        expr switch
        {
            SyntaxExpression.LiteralInteger literal => state.Push(new EvaluationValue.Int64(literal.Value)),
            SyntaxExpression.LiteralString literal => state.Push(new EvaluationValue.String(literal.Value)),

            SyntaxExpression.Application application => RunApplication(state, application),
            SyntaxExpression.Identifier identifier => RunIdentifier(state, identifier),
            SyntaxExpression.Conditional conditional => throw new NotImplementedException(),
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