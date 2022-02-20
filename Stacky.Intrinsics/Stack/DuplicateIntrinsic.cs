using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Stack;

public class DuplicateIntrinsic : IIntrinsic
{
    public string Name => "dup";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
         state = state.NewStackVariable(out var stack);
         state = state.NewVariable(new StackySort.Any(), out var input);

         type = new StackyType.Function(StackyType.MakeComposite(stack, input), StackyType.MakeComposite(stack, input, input));
         
         return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
         state = state.Pop(out var value);

         return state.Push(value).Push(value);
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
         var value = stack.Peek();
         stack = stack.Push(value);
         return stack;
    }
}