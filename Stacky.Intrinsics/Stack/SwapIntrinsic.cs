using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Stack;

public class SwapIntrinsic : IIntrinsic
{
    public string Name => "swap";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewVariable(new StackySort.Any(), out var type1);
        state = state.NewVariable(new StackySort.Any(), out var type2);

        type = new StackyType.Function(StackyType.MakeComposite(type1, type2), StackyType.MakeComposite(type2, type1));
        
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop(out var b);
        state = state.Pop(out var a);

        return state.Push(b).Push(a);
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        stack = stack.Pop(out var b, out var removeRootB);
        stack = stack.Pop(out var a, out var removeRootA);

        stack = stack.Push(b);
        removeRootB();

        stack = stack.Push(a);
        removeRootA();

        return stack;
    }
}