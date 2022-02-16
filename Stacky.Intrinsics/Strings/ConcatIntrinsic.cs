using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Strings;

public class ConcatIntrinsic : IIntrinsic
{
    public string Name => "concat";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        var str = new StackyType.String();
        
        type = new StackyType.Function(StackyType.MakeComposite(str, str), str);
        
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop(out var b);
        state = state.Pop(out var a);

        if (a is not EvaluationValue.String aStr)
        {
            throw new InvalidCastException($"Expected arg 0 to be String but got {a}");
        }
        if (b is not EvaluationValue.String bStr)
        {
            throw new InvalidCastException($"Expected arg 1 to be String but got {b}");
        }

        return state.Push(new EvaluationValue.String($"{aStr.Value}{bStr.Value}"));
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        var emitter = context.Emitter;

        var strlen = emitter.DefineNativeFunction("strlen", emitter.NativeFunctions.Strlen);
        var strcat = emitter.DefineNativeFunction("strcat", emitter.NativeFunctions.Strcat);

        // get length for new buffer
        stack = stack.Pop<CompilerType.String>(out var b, out var removeRootB);
        stack = stack.Pop<CompilerType.String>(out var a, out var removeRootA);

        var alength = emitter.Call(strlen, new CompilerType.Long(), a);
        var blength = emitter.Call(strlen, new CompilerType.Long(), b);

        // allocate memory
        var bufferLength = emitter.Add(emitter.Add(alength, blength), emitter.Literal(1));
        var buffer = context.Allocator.AllocateRaw(new CompilerType.String(), bufferLength);

        // concat
        emitter.Call(strcat, new CompilerType.String(), buffer, a);
        removeRootB();

        emitter.Call(strcat, new CompilerType.String(), buffer, b);
        removeRootA();

        // push to stack
        return stack.Push(buffer);
    }
}