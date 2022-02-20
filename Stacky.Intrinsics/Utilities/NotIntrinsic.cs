using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Utilities;

public class NotIntrinsic : IIntrinsic
{
    public string Name => "not";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        
        type = new StackyType.Function(
            StackyType.MakeComposite(stack, new StackyType.Boolean()),
            StackyType.MakeComposite(stack, new StackyType.Boolean()));
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<EvaluationValue.Boolean>(out var value);
        
        return state.Push(new EvaluationValue.Boolean(!value.Value));
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        stack = stack.Pop<CompilerType.Boolean>(out var value, out _);

        var result = context.Emitter.Not(value);
        
        return stack.Push(result);
    }
}