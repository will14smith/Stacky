using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public class OpenReadIntrinsic : IIntrinsic
{
    public string Name => "open-read";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        type = new StackyType.Function(new StackyType.String(), new FileInferenceType());
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<EvaluationValue.String>(out var pathStr);

        var path = pathStr.Value;

        var stream = File.OpenRead(path);
        var file = new FileEvaluationValue(stream);
        
        return state.Push(file);
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        throw new NotImplementedException();
    }
}