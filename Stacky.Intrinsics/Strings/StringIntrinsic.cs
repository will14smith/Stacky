using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Strings;

public class StringIntrinsic : IIntrinsic
{
    public string Name => "string";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        state = state.NewVariable(new StackySort.Printable(), out var input);
        
        type = new StackyType.Function(
            StackyType.MakeComposite(stack, input), 
            StackyType.MakeComposite(stack, new StackyType.String()));
        
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop(out var a);

        return a switch
        {
            EvaluationValue.Int64 i => state.Push(new EvaluationValue.String($"{i.Value}")),
            EvaluationValue.String s => state.Push(s),
            
            _ => throw new InvalidCastException($"arg 0 was an unexpected type: {a}")
        };
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        var type = stack.PeekType();

        return type switch
        {
            CompilerType.Long => CompileLong(context, stack),
            CompilerType.String => stack,

            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
    
    private static CompilerStack CompileLong(CompilerFunctionContext context, CompilerStack stack)
    {
        var emitter = context.Emitter;
        var sprintf = emitter.DefineNativeFunction("sprint", emitter.NativeFunctions.Sprintf);
        
        // allocate memory
        var bufferLength = (uint)(long.MinValue.ToString().Length + 1);
        var buffer = context.Allocator.AllocateRaw(new CompilerType.String(), bufferLength);

        // format long -> string
        var format = emitter.Literal("%lld");
        stack = stack.Pop<CompilerType.Long>(out var value, out _);
        emitter.Call(sprintf, new CompilerType.String(), buffer, format, value);
        
        // push to stack
        return stack.Push(buffer);
    }

}