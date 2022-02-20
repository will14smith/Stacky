using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public class CloseIntrinsic : IIntrinsic
{
    public string Name => "close";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        throw new NotImplementedException();
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<FileEvaluationValue>(out var file);

        file.Stream.Close();
        file.Stream.Dispose();

        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        throw new NotImplementedException();
    }
}