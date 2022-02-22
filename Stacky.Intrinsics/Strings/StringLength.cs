using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Syntax;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Strings;

public class StringLength : IIntrinsic
{
    public string Name => "length";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        
        var str = new StackyType.String();
        var len = new StackyType.Integer(true, SyntaxType.IntegerSize.S64);

        type = new StackyType.Function(
            StackyType.MakeComposite(stack, str), 
            StackyType.MakeComposite(stack, len) 
        );
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<EvaluationValue.String>(out var str);
        state = state.Push(new EvaluationValue.Int64(str.Value.Length));

        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        var emitter = context.Emitter;

        var strlen = emitter.DefineNativeFunction("strlen", emitter.NativeFunctions.Strlen);
        
        stack = stack.Pop<CompilerType.String>(out var str, out var removeRoot);

        var len = emitter.Call(strlen, new CompilerType.Long(), str);
        removeRoot();

        stack = stack.Push(len);

        return stack;
    }
}