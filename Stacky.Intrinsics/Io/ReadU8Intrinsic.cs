using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public class ReadU8Intrinsic : IIntrinsic
{
    public string Name => "read-u8";
    
    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        throw new NotImplementedException();
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<FileEvaluationValue>(out var file);
        
        var data = file.Stream.ReadByte();
        if (data == -1)
        {
            throw new NotImplementedException("handle EOF");
        }
        
        // TODO U8 not I64
        state = state.Push(new EvaluationValue.Int64((byte)data));

        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        throw new NotImplementedException();
    }
}