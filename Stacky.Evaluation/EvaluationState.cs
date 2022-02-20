using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using Stacky.Parsing.Syntax;

namespace Stacky.Evaluation;

public class EvaluationState
{
    private readonly SyntaxProgram _program;
    public ImmutableStack<EvaluationValue> Stack { get; }

    public EvaluationState(SyntaxProgram program) : this(program, ImmutableStack<EvaluationValue>.Empty) { }

    public EvaluationState(SyntaxProgram program, ImmutableStack<EvaluationValue> stack)
    {
        _program = program;
        Stack = stack;
    }


    public SyntaxFunction GetFunction(string name)
    {
        return _program.Functions.FirstOrDefault(x => x.Name.Value == name) ?? throw new Exception($"Function '{name}' was not declared");
    }
    
    public SyntaxStruct GetStruct(string name)
    {
        return _program.Structs.FirstOrDefault(x => x.Name.Value == name) ?? throw new Exception($"Struct '{name}' was not declared");
    }

    [Pure]
    public EvaluationState Push(EvaluationValue value)
    {
        return new EvaluationState(_program, Stack.Push(value));
    }   
    [Pure]
    public EvaluationState Pop(out EvaluationValue value)
    {
        var stack = Stack.Pop(out value);

        return new EvaluationState(_program, stack);
    }
}

public static class EvaluationStateExtensions
{
    public static EvaluationState Pop<T>(this EvaluationState state, out T value) where T : EvaluationValue
    {
        state = state.Pop(out var rawValue);
        if (rawValue is not T typedValue)
        {
            throw new InvalidCastException();
        }

        value = typedValue;
        return state;
    }
}