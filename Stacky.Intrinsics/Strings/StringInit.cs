using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Strings;

public class StringInit : IIntrinsic
{
    public string Name => "!@";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        state = state.NewVariable(new StackySort.Numeric(), out var len);
        
        var str = new StackyType.String();

        type = new StackyType.Function(
            StackyType.MakeComposite(stack, len), 
            StackyType.MakeComposite(stack, str) 
        );
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<EvaluationValue.Int64>(out var length);

        var str = new byte[length.Value];
        Array.Fill(str, (byte) ' ');
        
        state = state.Push(new EvaluationValue.String(str));
        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        var emitter = context.Emitter;

        stack = stack.Pop<CompilerType.Long>(out var length, out _);

        var bufferLength = emitter.Add(length, emitter.Literal(0));
        var buffer = context.Allocator.AllocateRaw(new CompilerType.String(), bufferLength);

        emitter.StoreIndex(buffer, length, emitter.LiteralByte(0));
        stack = stack.Push(buffer);
        
        return stack;
    }
}