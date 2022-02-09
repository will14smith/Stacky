namespace Stacky.Language.Evaluation;

public class EvaluationBuiltIn
{
    public static readonly IReadOnlyDictionary<string, Func<EvaluationState, EvaluationState>> Map = new Dictionary<string, Func<EvaluationState, EvaluationState>>
    {
        { "+", Int2((a, b) => a + b) },
        { "*", Int2((a, b) => a * b) },
        { "concat", StringConcat },
        { "toString", ToString },
        { "print", Print },
    };
    
    public static Func<EvaluationState, EvaluationState> Int2(Func<long, long, long> calculation)
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
    
    private static EvaluationState ToString(EvaluationState state)
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
}