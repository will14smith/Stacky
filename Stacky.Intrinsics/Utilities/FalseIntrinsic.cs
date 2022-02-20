using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Utilities;

public class FalseIntrinsic : IIntrinsic
{
    public string Name => "false";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);

        type = new StackyType.Function(stack, StackyType.MakeComposite(stack, new StackyType.Boolean()));
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state) => state.Push(new EvaluationValue.Boolean(false));

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack) => stack.Push(context.Emitter.Literal(false));
}