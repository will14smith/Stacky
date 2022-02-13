namespace Stacky.Parsing.Typing;

public static class InferenceIntrinsics
{
    public delegate InferenceState Intrinsic(InferenceState state, out StackyType type);

    private static readonly IReadOnlyDictionary<string, Intrinsic> Map = new Dictionary<string, Intrinsic>
    {
        { "drop", Drop },
        { "dup", Duplicate },
        { "invoke", Invoke },
        
        { "+", MathOp },
        { "-", MathOp },
        { "*", MathOp },
        { ">", CompOp },
        
        { "concat", Concat },
        { "string", String },
        { "print", Print },
    };
    
    public static bool TryInfer(string name, out Intrinsic? intrinsic) => Map.TryGetValue(name, out intrinsic);

    private static InferenceState Drop(InferenceState state, out StackyType type)
    {
        state = state.NewVariable(new StackySort.Any(), out var input);

        type = new StackyType.Function(input, new StackyType.Void());
        
        return state;
    }

    private static InferenceState Duplicate(InferenceState state, out StackyType type)
    {
        state = state.NewVariable(new StackySort.Any(), out var input);

        type = new StackyType.Function(input, StackyType.MakeComposite(input, input));
        
        return state;
    }
    
    private static InferenceState Invoke(InferenceState state, out StackyType type)
    {
        state = state.NewVariable(new StackySort.Any(), out var input);
        state = state.NewVariable(new StackySort.Any(), out var output);

        var fnType = new StackyType.Function(input, output);
        
        type = new StackyType.Function(StackyType.MakeComposite(input, fnType), output);
        
        return state;
    }

    private static InferenceState MathOp(InferenceState state, out StackyType type)
    {
        state = state.NewVariable(new StackySort.Numeric(), out var input);

        type = new StackyType.Function(StackyType.MakeComposite(input, input), input);
        
        return state;
    }
    
    private static InferenceState CompOp(InferenceState state, out StackyType type)
    {
        state = state.NewVariable(new StackySort.Comparable(), out var input);

        type = new StackyType.Function(StackyType.MakeComposite(input, input), new StackyType.Boolean());
        
        return state;
    }


    private static InferenceState Concat(InferenceState state, out StackyType type)
    {
        var str = new StackyType.String();
        
        type = new StackyType.Function(StackyType.MakeComposite(str, str), str);
        
        return state;

    }
    private static InferenceState String(InferenceState state, out StackyType type)
    {
        state = state.NewVariable(new StackySort.Printable(), out var input);
        
        type = new StackyType.Function(input, new StackyType.String());
        
        return state;
    }   
    private static InferenceState Print(InferenceState state, out StackyType type)
    {
        state = state.NewVariable(new StackySort.Printable(), out var input);
        
        type = new StackyType.Function(input, new StackyType.Void());
        
        return state;
    }
}