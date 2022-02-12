namespace Stacky.Evaluation;

public class EvaluationBuiltIn
{
    public delegate EvaluationState BuiltIn(EvaluationState state);
    
    public static readonly IReadOnlyDictionary<string, BuiltIn> Map = new Dictionary<string, BuiltIn>
    {
        { "drop", s => s.Pop(out _) },
        { "dup", Duplicate },
        
        { "+", IntBinary((a, b) => a + b) },
        { "-", IntBinary((a, b) => a - b) },
        { "*", IntBinary((a, b) => a * b) },
        { ">", IntBinary((a, b) => a > b) },
        { "concat", StringConcat },
        { "string", String },
        { "print", Print },
        { "invoke", Invoke },
        
        {  "if", EvaluationFlow.If },
        {  "if-else", EvaluationFlow.IfElse },
        {  "while", EvaluationFlow.While },
        
        { "true", s => s.Push(new EvaluationValue.Boolean(true)) },
        { "false", s => s.Push(new EvaluationValue.Boolean(false)) },
    };

    private static EvaluationState Duplicate(EvaluationState state)
    {
        state = state.Pop(out var value);

        return state.Push(value).Push(value);
    }

    public static BuiltIn IntBinary(Func<long, long, long> calculation)
    {
        return state =>
        {
            state = state.Pop(out var b);
            state = state.Pop(out var a);

            if (a is not EvaluationValue.Int64 aInt)
            {
                throw new InvalidCastException($"Expected arg 0 to be Int64 but got {a}");
            }

            if (b is not EvaluationValue.Int64 bInt)
            {
                throw new InvalidCastException($"Expected arg 1 to be Int64 but got {b}");
            }

            var result = calculation(aInt.Value, bInt.Value);
            return state.Push(new EvaluationValue.Int64(result));
        };
    }
    
    public static BuiltIn IntBinary(Func<long, long, bool> calculation)
    {
        return state =>
        {
            state = state.Pop(out var b);
            state = state.Pop(out var a);

            if (a is not EvaluationValue.Int64 aInt)
            {
                throw new InvalidCastException($"Expected arg 0 to be Int64 but got {a}");
            }

            if (b is not EvaluationValue.Int64 bInt)
            {
                throw new InvalidCastException($"Expected arg 1 to be Int64 but got {b}");
            }

            var result = calculation(aInt.Value, bInt.Value);
            return state.Push(new EvaluationValue.Boolean(result));
        };
    }

    
    private static EvaluationState StringConcat(EvaluationState state)
    {
        state = state.Pop(out var b);
        state = state.Pop(out var a);

        if (a is not EvaluationValue.String aStr)
        {
            throw new InvalidCastException($"Expected arg 0 to be String but got {a}");
        }
        if (b is not EvaluationValue.String bStr)
        {
            throw new InvalidCastException($"Expected arg 1 to be String but got {b}");
        }

        return state.Push(new EvaluationValue.String($"{aStr.Value}{bStr.Value}"));
    }
    
    private static EvaluationState String(EvaluationState state)
    {
        state = state.Pop(out var a);

        return a switch
        {
            EvaluationValue.Int64 i => state.Push(new EvaluationValue.String($"{i.Value}")),
            EvaluationValue.String s => state.Push(s),
            
            _ => throw new InvalidCastException($"arg 0 was an unexpected type: {a}")
        };
    }
    
    private static EvaluationState Print(EvaluationState state)
    {
        state = state.Pop(out var a);
        
        switch (a)
        {
            case EvaluationValue.Int64 i: Console.WriteLine(i.Value); break;
            case EvaluationValue.String s: Console.WriteLine(s.Value); break;
            
            default: throw new InvalidCastException($"arg 0 was an unexpected type: {a}");
        }
        
        return state;
    }
    
    private static EvaluationState Invoke(EvaluationState state)
    {
        state = state.Pop(out var a);

        if (a is not EvaluationValue.Function function)
        {
            throw new InvalidCastException($"Expected arg 0 to be Function but got {a}");
        }

        return Evaluator.RunExpression(state, function.Body);
    }
}