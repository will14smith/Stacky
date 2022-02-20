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
        throw new NotImplementedException();
    }
}