namespace Stacky.Parsing.Typing;

public static class InferenceIntrinsics
{
    public delegate InferenceState Intrinsic(InferenceState state, out StackyType type);

    private static readonly IReadOnlyDictionary<string, Intrinsic> Map = new Dictionary<string, Intrinsic>
    {
        { "+", MathOp },
        { "-", MathOp },
        { "*", MathOp },
        { ">", CompOp },
        
        { "concat", Concat },
        { "string", String },
        { "print", Print },
    };
    
    public static bool TryInfer(string name, out Intrinsic? intrinsic) => Map.TryGetValue(name, out intrinsic);

    private static InferenceState MathOp(InferenceState state, out StackyType type)
    {
        state = state.NewVariable(new StackySort.Numeric(), out var input);

        type = new StackyType.Function(new[] { input, input }, new[] { input });
        
        return state;
    }
    
    private static InferenceState CompOp(InferenceState state, out StackyType type)
    {
        state = state.NewVariable(new StackySort.Comparable(), out var input);

        type = new StackyType.Function(new[] { input, input }, new[] { new StackyType.Boolean() });
        
        return state;
    }


    private static InferenceState Concat(InferenceState state, out StackyType type)
    {
        type = new StackyType.Function(new[] { new StackyType.String(), new StackyType.String() }, new [] { new StackyType.String() });
        
        return state;

    }
    private static InferenceState String(InferenceState state, out StackyType type)
    {
        state = state.NewVariable(new StackySort.Printable(), out var input);
        
        type = new StackyType.Function(new[] { input }, new [] { new StackyType.String() });
        
        return state;
    }   
    private static InferenceState Print(InferenceState state, out StackyType type)
    {
        state = state.NewVariable(new StackySort.Printable(), out var input);
        
        type = new StackyType.Function(new[] { input }, Array.Empty<StackyType>());
        
        return state;
    }
}