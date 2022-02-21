using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public class CloseIntrinsic : IIntrinsic
{
    public string Name => "close";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);

        type = new StackyType.Function(StackyType.MakeComposite(stack, new FileInferenceType()), stack);
        return state;
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
        var emitter = context.Emitter;
        
        // int fclose(FILE* file);
        var fclose = emitter.DefineNativeFunction("fclose", emitter.NativeFunctions.Fclose);
        
        stack = stack.Pop<FileCompilerType>(out var file, out var removeRoot);
        
        var result = emitter.Call(fclose, new CompilerType.Long(), file);
        // TODO handle non-zero return
        removeRoot();
        
        return stack;
    }
}