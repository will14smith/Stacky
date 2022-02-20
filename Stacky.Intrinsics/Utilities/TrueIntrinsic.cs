using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Utilities;

public class TrueIntrinsic : IIntrinsic
{
    public string Name => "true";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);

        type = new StackyType.Function(stack, StackyType.MakeComposite(stack, new StackyType.Boolean()));
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state) => state.Push(new EvaluationValue.Boolean(true));

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack) => stack.Push(context.Emitter.Literal(true));
}