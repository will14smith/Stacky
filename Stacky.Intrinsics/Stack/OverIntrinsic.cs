using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Stack;

public class OverIntrinsic : IIntrinsic
{
    public string Name => "over";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        state = state.NewVariable(new StackySort.Any(), out var a);
        state = state.NewVariable(new StackySort.Any(), out var b);

        type = new StackyType.Function(StackyType.MakeComposite(stack, a, b), StackyType.MakeComposite(stack, a, b, a));
         
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop(out var b);
        state = state.Pop(out var a);

        return state.Push(a).Push(b).Push(a);
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        throw new NotImplementedException();
    }
}