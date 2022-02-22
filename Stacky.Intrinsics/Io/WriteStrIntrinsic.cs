using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public class WriteStrIntrinsic : IIntrinsic
{
    public string Name => "write-str";
    
    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);

        type = new StackyType.Function(
            StackyType.MakeComposite(stack, new FileInferenceType(), new StackyType.String()),
            stack
        );
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<EvaluationValue.String>(out var str);
        state = state.Pop<FileEvaluationValue>(out var file);

        using var writer = new StreamWriter(file.Stream, leaveOpen: true);
        writer.Write(str.StringValue);

        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        var emitter = context.Emitter;
        
        // int fprintf (FILE* stream, const char* format, ...);
        var fprintf = emitter.DefineNativeFunction("fprintf", emitter.NativeFunctions.Fprintf);
        
        stack = stack.Pop<CompilerType.String>(out var value, out var removeRootStr);
        stack = stack.Pop<FileCompilerType>(out var file, out var removeRootFile);

        var format = emitter.Literal("%s");
        
        var result = emitter.Call(fprintf, new CompilerType.Int(), file, format, value);
        // TODO handle < 0 return

        removeRootStr();
        removeRootFile();

        return stack;
    }
}