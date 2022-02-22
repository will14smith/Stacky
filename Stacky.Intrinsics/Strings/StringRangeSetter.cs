using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Strings;

public class StringRangeSetter : IIntrinsic
{
    public string Name => "!!~";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        state = state.NewVariable(new StackySort.Numeric(), out var index);
        
        var str = new StackyType.String();

        type = new StackyType.Function(
            StackyType.MakeComposite(stack, str, str, index), 
            StackyType.MakeComposite(stack, str) 
        );
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<EvaluationValue.Int64>(out var index);
        state = state.Pop<EvaluationValue.String>(out var update);
        state = state.Pop<EvaluationValue.String>(out var str);

        Array.Copy(update.Value, 0, str.Value, index.Value, update.Value.Length);
        
        state = state.Push(str);
        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        var emitter = context.Emitter;

        stack = stack.Pop<CompilerType.Long>(out var index, out _);
        stack = stack.Pop<CompilerType.String>(out var update, out var removeRootUpdate);
        stack = stack.Pop<CompilerType.String>(out var str, out var removeRootStr);
        
        var strlen = emitter.DefineNativeFunction("strlen", emitter.NativeFunctions.Strlen);
        var length = emitter.Call(strlen, new CompilerType.Long(), update);

        emitter.Copy(str, index, update, emitter.Literal(0), length);
        removeRootUpdate();
        stack = stack.Push(str);
        removeRootStr();
        
        return stack;
    }
}