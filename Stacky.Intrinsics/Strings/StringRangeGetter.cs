using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Strings;

public class StringRangeGetter : IIntrinsic
{
    public string Name => "!!";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        state = state.NewVariable(new StackySort.Numeric(), out var index);
        
        var str = new StackyType.String();

        type = new StackyType.Function(
            StackyType.MakeComposite(stack, str, index, index), 
            StackyType.MakeComposite(stack, str) 
        );
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<EvaluationValue.Int64>(out var indexEnd);
        state = state.Pop<EvaluationValue.Int64>(out var indexStart);
        state = state.Pop<EvaluationValue.String>(out var str);

        var newStr = new byte[indexEnd.Value - indexStart.Value];
        Array.Copy(str.Value, indexStart.Value, newStr, 0, newStr.Length);
        
        state = state.Push(new EvaluationValue.String(newStr));
        return state;

    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        var emitter = context.Emitter;

        stack = stack.Pop<CompilerType.Long>(out var indexEnd, out _);
        stack = stack.Pop<CompilerType.Long>(out var indexStart, out _);
        stack = stack.Pop<CompilerType.String>(out var str, out var removeRoot);

        var length = emitter.Sub(indexEnd, indexStart);
        var bufferLength = emitter.Add(length, emitter.Literal(1));
        var buffer = context.Allocator.AllocateRaw(new CompilerType.String(), bufferLength);
        
        emitter.Copy(buffer, emitter.Literal(0), str, indexStart, length);
        emitter.StoreIndex(buffer, length, emitter.LiteralByte(0));
        removeRoot();
        
        return stack.Push(buffer);
    }
}