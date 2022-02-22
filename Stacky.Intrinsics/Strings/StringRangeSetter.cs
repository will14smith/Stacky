using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Strings;

public class StringRangeSetter : IIntrinsic
{
    public string Name => "!!~";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        state = state.NewVariable(new StackySort.Numeric(), out var index);
        
        var str = new StackyType.String();

        type = new StackyType.Function(
            StackyType.MakeComposite(stack, str, str, index), 
            StackyType.MakeComposite(stack, str) 
        );
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<EvaluationValue.Int64>(out var index);
        state = state.Pop<EvaluationValue.String>(out var update);
        state = state.Pop<EvaluationValue.String>(out var str);

        Array.Copy(update.Value, 0, str.Value, index.Value, update.Value.Length);
        
        state = state.Push(str);
        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack) => throw new NotImplementedException();
}