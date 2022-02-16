using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Stack;

public class DropIntrinsic : IIntrinsic
{
    public string Name => "drop";
    
    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewVariable(new StackySort.Any(), out var input);
        
        type = new StackyType.Function(input, new StackyType.Void());
        
        return state;
    }
    
    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        return state.Pop(out _);
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        stack = stack.Pop(out _, out var removeRoot);
        
        removeRoot();
        
        return stack;
    }
}