using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public class IsEofIntrinsic : IIntrinsic
{
    public string Name => "is-eof";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        type = new StackyType.Function(new FileInferenceType(), new StackyType.Boolean());
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<FileEvaluationValue>(out var file);

        var isEof = file.Stream.Position >= file.Stream.Length;
        state = state.Push(new EvaluationValue.Boolean(isEof));

        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        throw new NotImplementedException();
    }
}