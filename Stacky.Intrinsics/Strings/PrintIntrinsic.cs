using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Strings;

public class PrintIntrinsic : IIntrinsic
{
    public string Name => "print";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        state = state.NewVariable(new StackySort.Printable(), out var input);

        type = new StackyType.Function(StackyType.MakeComposite(stack, input), stack);

        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
         state = state.Pop(out var a);
         
         switch (a)
         {
             case EvaluationValue.Boolean i: Console.WriteLine(i.Value); break;
             case EvaluationValue.Int64 i: Console.WriteLine(i.Value); break;
             case EvaluationValue.String s: Console.WriteLine(s.Value); break;
             
             default: throw new InvalidCastException($"arg 0 was an unexpected type: {a}");
         }
         
         return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        var type = stack.PeekType();

        return type switch
        {
            // TODO update to (x ? "True" : "False")
            CompilerType.Boolean => Printf(context, stack, "%d\n"),
            CompilerType.Int => Printf(context, stack, "%d\n"),
            CompilerType.Long => Printf(context, stack, "%lld\n"),
            CompilerType.String => Printf(context, stack, "%s\n"),
            
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private static CompilerStack Printf(CompilerFunctionContext context, CompilerStack stack, string formatString)
    {
        var format = context.Emitter.Literal(formatString);
        stack = stack.Pop(out var value, out _);

        var printf = context.Emitter.DefineNativeFunction("printf", context.Emitter.NativeFunctions.Printf);
        context.Emitter.Call(printf, new CompilerType.Long(), format, value);

        return stack;
    }
}