using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Utilities;

public class InvokeIntrinsic : IIntrinsic
{
    public string Name => "invoke";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var inStack);
        state = state.NewStackVariable(out var outStack);

        var fnType = new StackyType.Function(inStack, outStack);
        
        type = new StackyType.Function(StackyType.MakeComposite(inStack, fnType), outStack);
        
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop(out var a);

        if (a is not EvaluationValue.Closure function)
        {
            throw new InvalidCastException($"Expected arg 0 to be Function but got {a}");
        }

        return evaluator.RunClosure(state, function);
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        stack = stack.Pop(out var closure, out var removeClosureRoot); 
        stack = context.Invoke(stack, closure);
        removeClosureRoot();
        return stack;
    }
}