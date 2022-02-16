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
        if (identifier.Value.Length > 1)
        {
            switch (identifier.Value[0])
            {
                case '@': return RunInit(ref state, identifier);                
                case '#': return RunGetter(ref state, identifier);                
                case '~': return RunSetter(ref state, identifier);                
            }
        }

        if (EvaluationBuiltIn.Map.TryGetValue(identifier.Value, out var handler))
        {
            return handler(state);
        }
        
        var function = state.GetFunction(identifier.Value);
        return RunFunction(state, function);

    }

    private static EvaluationState RunInit(ref EvaluationState state, SyntaxExpression.Identifier identifier)
    {
        var structName = identifier.Value[1..];
        var structDefinition = state.GetStruct(structName); 
        
        return state.Push(new EvaluationValue.Struct(structDefinition));
    }

    private static EvaluationState RunGetter(ref EvaluationState state, SyntaxExpression.Identifier identifier)
    {
        var fieldName = identifier.Value[1..];

        state = state.Pop(out var a);
        
        if (a is not EvaluationValue.Struct structValue)
        {
            throw new InvalidCastException($"Expected arg 0 to be Struct but got {a}");
        }

        var value = structValue.Get(fieldName);

        return state.Push(value);
    }

    private static EvaluationState RunSetter(ref EvaluationState state, SyntaxExpression.Identifier identifier)
    {
        var fieldName = identifier.Value[1..];

        state = state.Pop(out var value);
        state = state.Pop(out var a);
        
        if (a is not EvaluationValue.Struct structValue)
        {
            throw new InvalidCastException($"Expected arg 0 to be Struct but got {a}");
        }

        structValue = structValue.Update(fieldName, value);

        return state.Push(structValue);
    }
}