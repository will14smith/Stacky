using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Flow;

public class IfIntrinsic : IIntrinsic
{
    public string Name => "if";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);

        var condition = new StackyType.Boolean();
        var trueFunc = new StackyType.Function(stack, stack);
        
        type = new StackyType.Function(
            StackyType.MakeComposite(stack, condition, trueFunc),
            stack
        );
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop(out var trueValue);
        if (trueValue is not EvaluationValue.Closure trueFunc)
        {
            throw new InvalidCastException($"Expected arg 0 to be Function but got {trueValue}");
        }

        state = state.Pop(out var condition);
        if (condition is not EvaluationValue.Boolean conditionBool)
        {
            throw new InvalidCastException($"Expected arg 1 to be Boolean but got {condition}");
        }

        return conditionBool.Value ? evaluator.RunClosure(state, trueFunc) : state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        var emitter = context.Emitter;
        
        stack = stack.Pop(out var trueClosure, out var removeTrueClosureRoot);
        stack = stack.Pop<CompilerType.Boolean>(out var condition, out _);

        var mergeBlock = emitter.CreateBlockInCurrent("merge");
        var trueBlock = emitter.CreateBlockInCurrent("true");

        emitter.Branch(condition, trueBlock, mergeBlock);
        
        emitter.BeginBlock(trueBlock);
        var trueStack = context.Invoke(stack, trueClosure);
        emitter.Branch(mergeBlock);
        
        emitter.BeginBlock(mergeBlock);

        removeTrueClosureRoot();
        
        return trueStack;
    }
}