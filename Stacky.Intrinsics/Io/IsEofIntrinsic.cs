using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public class IsEofIntrinsic : IIntrinsic
{
    public string Name => "is-eof";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);

        type = new StackyType.Function(StackyType.MakeComposite(stack, new FileInferenceType()), StackyType.MakeComposite(stack, new StackyType.Boolean()));
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
        var emitter = context.Emitter;
        
        // int feof(FILE* file)
        var feof = emitter.DefineNativeFunction("feof", emitter.NativeFunctions.Feof);

        stack = stack.Pop<FileCompilerType>(out var file, out var removeRoot);
        var result = emitter.Call(feof, new CompilerType.Int(), file);
        removeRoot();

        var boolResult = emitter.NotEqual(result, emitter.Literal(0));
        
        return stack.Push(boolResult);
    }

}