using System.Collections.Immutable;
using Stacky.Parsing.Syntax;

namespace Stacky.Evaluation;

public class Evaluator
{
    private readonly SyntaxProgram _program;
    
    public EvaluationIntrinsicRegistry Intrinsics { get; } = new();

    public Evaluator(SyntaxProgram program)
    {
        _program = program;
    }

    public ImmutableStack<EvaluationValue> Run() => Run(ImmutableStack<EvaluationValue>.Empty);
    public ImmutableStack<EvaluationValue> Run(ImmutableStack<EvaluationValue> initial)
    {
        var state = new EvaluationState(_program, initial, ImmutableStack<IReadOnlyDictionary<string, EvaluationValue>>.Empty);
        var function = state.GetFunction("main");

        state = RunFunction(state, function);

        return state.Stack;
    }
    
    private EvaluationState RunFunction(EvaluationState state, SyntaxFunction function) => RunExpression(state, function.Body);

    public EvaluationState RunExpression(EvaluationState state, SyntaxExpression expr) =>
        expr switch
        {
            SyntaxExpression.LiteralInteger literal => state.Push(new EvaluationValue.Int64(literal.Value)),
            SyntaxExpression.LiteralString literal => state.Push(new EvaluationValue.String(literal.Value)),
            SyntaxExpression.Function function => state.Push(new EvaluationValue.Function(function.Body)),
            
            SyntaxExpression.Application application => RunApplication(state, application),
            SyntaxExpression.Identifier identifier => RunIdentifier(state, identifier),
            SyntaxExpression.Binding binding => RunBinding(state, binding),
            
            _ => throw new ArgumentOutOfRangeException(nameof(expr))
        };
    
    private EvaluationState RunApplication(EvaluationState state, SyntaxExpression.Application application) => application.Expressions.Aggregate(state, RunExpression);

    private EvaluationState RunIdentifier(EvaluationState state, SyntaxExpression.Identifier identifier)
    {
        if (state.TryLookupBinding(identifier.Value, out var binding))
        {
            return state.Push(binding!);
        }
        
        if (identifier.Value.Length > 1)
        {
            switch (identifier.Value[0])
            {
                case '@': return RunInit(ref state, identifier);                
                case '#': return RunGetter(ref state, identifier);                
                case '~': return RunSetter(ref state, identifier);                
            }
        }

        if (Intrinsics.TryGetIntrinsic(identifier.Value, out var handler))
        {
            return handler!.Evaluate(this, state);
        }
        
        var function = state.GetFunction(identifier.Value);
        return RunFunction(state, function);

    }

    private EvaluationState RunBinding(EvaluationState state, SyntaxExpression.Binding binding)
    {
        var values = new Dictionary<string, EvaluationValue>();

        for (var i = binding.Names.Count - 1; i >= 0; i--)
        {
            var name = binding.Names[i];
            state = state.Pop(out var value);

            if (!values.ContainsKey(name.Value))
            {
                values[name.Value] = value;
            }
        }

        state = state.PushBindings(values);
        state = RunExpression(state, binding.Body);
        state = state.PopBindings();
        
        return state;
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