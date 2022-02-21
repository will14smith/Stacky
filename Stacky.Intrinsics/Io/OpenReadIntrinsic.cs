using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public class OpenReadIntrinsic : IIntrinsic
{
    public string Name => "open-read";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        
        type = new StackyType.Function(StackyType.MakeComposite(stack, new StackyType.String()), StackyType.MakeComposite(stack, new FileInferenceType()));
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
        var emitter = context.Emitter;
        
        // FILE* fopen(const char* path, const char* mode);
        var fopen = emitter.DefineNativeFunction("fopen", emitter.NativeFunctions.Fopen);
        
        stack = stack.Pop<CompilerType.String>(out var path, out var removeRoot);
        var mode = emitter.Literal("r");

        var result = emitter.Call(fopen, new FileCompilerType(), path, mode);
        // TODO handle NULL return
        
        removeRoot();
        
        return stack.Push(result);
    }
}