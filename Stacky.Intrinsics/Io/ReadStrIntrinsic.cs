using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public class ReadStrIntrinsic : IIntrinsic
{
    public string Name => "read-str";
    
    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        throw new NotImplementedException();
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<FileEvaluationValue>(out var file);

        using var reader = new StreamReader(file.Stream, leaveOpen: true);
        var str = reader.ReadToEnd();
        state = state.Push(new EvaluationValue.String(str));

        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        throw new NotImplementedException();
    }
}