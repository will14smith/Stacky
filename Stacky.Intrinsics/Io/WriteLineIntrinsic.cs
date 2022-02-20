using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public class WriteLineIntrinsic : IIntrinsic
{
    public string Name => "write-line";
    
    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        throw new NotImplementedException();
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<EvaluationValue.String>(out var str);
        state = state.Pop<FileEvaluationValue>(out var file);

        using var writer = new StreamWriter(file.Stream, leaveOpen: true);
        writer.Write(str.Value);
        writer.Write('\n');

        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        throw new NotImplementedException();
    }
}