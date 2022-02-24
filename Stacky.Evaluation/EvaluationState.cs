using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using Stacky.Parsing.Typing;

namespace Stacky.Evaluation;

public class EvaluationState
{
    private readonly TypedProgram _program;
    public ImmutableStack<EvaluationValue> Stack { get; }
    public ImmutableStack<IReadOnlyDictionary<string, EvaluationValue>> Bindings { get; }

    public EvaluationState(TypedProgram program) : this(program, ImmutableStack<EvaluationValue>.Empty, ImmutableStack<IReadOnlyDictionary<string, EvaluationValue>>.Empty) { }

    public EvaluationState(TypedProgram program, ImmutableStack<EvaluationValue> stack, ImmutableStack<IReadOnlyDictionary<string, EvaluationValue>> bindings)
    {
        _program = program;
        Stack = stack;
        Bindings = bindings;
    }


    public TypedFunction GetFunction(string name)
    {
        return _program.Functions.FirstOrDefault(x => x.Name.Value == name) ?? throw new Exception($"Function '{name}' was not declared");
    }
    
    public TypedStruct GetStruct(string name)
    {
        return _program.Structs.FirstOrDefault(x => x.Name.Value == name) ?? throw new Exception($"Struct '{name}' was not declared");
    }

    [Pure]
    public EvaluationState Push(EvaluationValue value)
    {
        return new EvaluationState(_program, Stack.Push(value), Bindings);
    }   
    [Pure]
    public EvaluationState Pop(out EvaluationValue value)
    {
        var stack = Stack.Pop(out value);

        return new EvaluationState(_program, stack, Bindings);
    }

    public EvaluationState PushBindings(IReadOnlyDictionary<string, EvaluationValue> bindings)
    {
        return new EvaluationState(_program, Stack, Bindings.Push(bindings));
    }

    public EvaluationState PopBindings()
    {
        return new EvaluationState(_program, Stack, Bindings.Pop(out _));
    }

    public bool TryLookupBinding(string name, out EvaluationValue? value)
    {
        foreach (var binding in Bindings)
        {
            if (binding.TryGetValue(name, out value))
            {
                return true;
            }
        }

        value = default;
        return false;
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