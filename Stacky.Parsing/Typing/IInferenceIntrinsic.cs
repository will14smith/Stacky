namespace Stacky.Parsing.Typing;

public interface IInferenceIntrinsic
{
    InferenceState Infer(InferenceState state, out StackyType type);
}