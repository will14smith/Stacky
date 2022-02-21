using System.Text;
using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Syntax;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public class WriteU8Intrinsic : IIntrinsic
{
    public string Name => "write-u8";
    
    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);

        type = new StackyType.Function(
            StackyType.MakeComposite(stack, new FileInferenceType(), new StackyType.Integer(false, SyntaxType.IntegerSize.S8)),
            stack
        );
        return state;
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
        var emitter = context.Emitter;
        
        // int fputc(int c, FILE* stream);
        var fputc = emitter.DefineNativeFunction("fputc", emitter.NativeFunctions.Fputc);
        
        stack = stack.Pop<CompilerType.Byte>(out var value, out _);
        stack = stack.Pop<FileCompilerType>(out var file, out var removeRoot);

        var result = emitter.Call(fputc, new CompilerType.Int(), value, file);
        // TODO handle EOF return
        
        removeRoot();

        return stack;
    }
}