using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Arithmetic;

public abstract class BinaryArithmeticIntrinsic : IIntrinsic
{
    public abstract string Name { get; }

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        state = state.NewVariable(new StackySort.Numeric(), out var input);

        type = new StackyType.Function(StackyType.MakeComposite(stack, input, input), StackyType.MakeComposite(stack, input));
        
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
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

        var result = Evaluate(aInt.Value, bInt.Value);
        return state.Push(new EvaluationValue.Int64(result));
    }
    protected abstract long Evaluate(long a, long b);

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        stack = stack.Pop<CompilerType.Long>(out var b, out _);
        stack = stack.Pop<CompilerType.Long>(out var a, out _);

        return stack.Push(Compile(context, a, b));
    }
    protected abstract CompilerValue Compile(CompilerFunctionContext context, CompilerValue a, CompilerValue b);
}