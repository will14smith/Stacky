using System.Text;
using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Strings;

public class Char : IIntrinsic
{
    public string Name => "char";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        state = state.NewVariable(new StackySort.Numeric(), out var input);
        
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
            EvaluationValue.Int64 i => state.Push(new EvaluationValue.String(new [] { (byte) i.Value })),
            
            _ => throw new InvalidCastException($"arg 0 was an unexpected type: {a}")
        };
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        var type = stack.PeekType();

        return type switch
        {
            CompilerType.Byte => CompileFormat(context, stack),
            CompilerType.Long => CompileFormat(context, stack),

            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
    
    private static CompilerStack CompileFormat(CompilerFunctionContext context, CompilerStack stack)
    {
        var emitter = context.Emitter;
        var sprintf = emitter.DefineNativeFunction("sprintf", emitter.NativeFunctions.Sprintf);
        
        // allocate memory
        var buffer = context.Allocator.AllocateRaw(new CompilerType.String(), 2);

        // format num -> char
        var format = emitter.Literal("%c");
        stack = stack.Pop<INumericType>(out var valueNum, out _);
        var value = emitter.Truncate(valueNum, new CompilerType.Byte());
        emitter.Call(sprintf, new CompilerType.String(), buffer, format, value);
        
        // push to stack
        return stack.Push(buffer);
    }

}