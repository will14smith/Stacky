using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Syntax;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public class ReadU8Intrinsic : IIntrinsic
{
    public string Name => "read-u8";
    
    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        
        type = new StackyType.Function(StackyType.MakeComposite(stack, new FileInferenceType()), StackyType.MakeComposite(stack, new StackyType.Integer(false, SyntaxType.IntegerSize.S8)));
        return state;
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
        var emitter = context.Emitter;
        
        // int fgetc(FILE* stream);
        var fgetc = emitter.DefineNativeFunction("fgetc", emitter.NativeFunctions.Fgetc);
        
        stack = stack.Pop<FileCompilerType>(out var file, out var removeRoot);

        var result = emitter.Call(fgetc, new CompilerType.Int(), file);
        // TODO handle EOF return
        
        removeRoot();

        var value = emitter.Truncate(result, new CompilerType.Byte());
        return stack.Push(value);
    }
}