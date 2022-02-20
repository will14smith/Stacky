using System.Text;
using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public class WriteU8Intrinsic : IIntrinsic
{
    public string Name => "write-u8";
    
    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        throw new NotImplementedException();
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        // TODO U8 not I64
        state = state.Pop<EvaluationValue.Int64>(out var u8);
        state = state.Pop<FileEvaluationValue>(out var file);

        using var writer = new BinaryWriter(file.Stream, Encoding.UTF8, true);
        writer.Write((byte) u8.Value);

        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        throw new NotImplementedException();
    }
}