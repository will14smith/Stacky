using System.Text;
using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public class ReadLineIntrinsic : IIntrinsic
{
    public string Name => "read-line";
    
    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        
        type = new StackyType.Function(StackyType.MakeComposite(stack, new FileInferenceType()), StackyType.MakeComposite(stack, new StackyType.String()));
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<FileEvaluationValue>(out var file);
        
        var buffer = new StringBuilder();

        while (true)
        {
            var data = file.Stream.ReadByte();

            if (data == -1)
            {
                if (buffer.Length == 0)
                {
                    throw new NotImplementedException("handle EOF");
                }
                
                break;
            }

            if (data == '\n')
            {
                break;
            }

            buffer.Append((char)data);
        }

        state = state.Push(new EvaluationValue.String(buffer.ToString()));

        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        var emitter = context.Emitter;
        
        // char* fgets (char* dst, int count, FILE* file);
        var fgets = emitter.DefineNativeFunction("fgets", emitter.NativeFunctions.Fgets);

        // TODO handle lines > buffer
        var bufferSize = 100;
        var buffer = context.Allocator.AllocateRaw(new CompilerType.String(), bufferSize);
        
        stack = stack.Pop<FileCompilerType>(out var file, out var removeRoot);

        var result = emitter.Call(fgets, new CompilerType.String(), buffer, emitter.Literal(bufferSize), file);
        // TODO handle NULL return
        
        removeRoot();
        
        return stack.Push(result);
    }
}