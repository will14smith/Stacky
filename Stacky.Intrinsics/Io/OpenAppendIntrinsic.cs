using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public class OpenAppendIntrinsic : IIntrinsic
{
    public string Name => "open-append";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        throw new NotImplementedException();
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<EvaluationValue.String>(out var pathStr);

        var path = pathStr.Value;

        var stream = File.Open(path, FileMode.Append, FileAccess.Write);
        var file = new FileEvaluationValue(stream);
        
        return state.Push(file);
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        throw new NotImplementedException();
    }
}