using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public class ReadStrIntrinsic : IIntrinsic
{
    public string Name => "read-str";
    
    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        
        type = new StackyType.Function(StackyType.MakeComposite(stack, new FileInferenceType()), StackyType.MakeComposite(stack, new StackyType.String()));
        return state;
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
        var emitter = context.Emitter;
        
        // size_t fread(void* dst, size_t size, size_t count, FILE* stream);
        var fread = emitter.DefineNativeFunction("fread", emitter.NativeFunctions.Fread);

        // TODO handle files > buffer
        var bufferSize = 1024L;
        var buffer = context.Allocator.AllocateRaw(new CompilerType.String(), bufferSize);
        
        stack = stack.Pop<FileCompilerType>(out var file, out var removeRoot);

        var count = emitter.Call(fread, new CompilerType.Long(), buffer, emitter.Literal(1L), emitter.Literal(bufferSize), file);
        // TODO handle NULL return
        
        removeRoot();
        
        return stack.Push(buffer);
    }
}