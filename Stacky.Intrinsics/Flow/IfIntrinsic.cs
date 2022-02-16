using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Flow;

public class IfIntrinsic : IIntrinsic
{
    public string Name => "if";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewVariable(new StackySort.Any(), out var input);

        var condition = new StackyType.Boolean();
        var trueFunc = new StackyType.Function(input, input);
        
        type = new StackyType.Function(
            StackyType.MakeComposite(input, condition, trueFunc),
            input
        );
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop(out var trueValue);
        if (trueValue is not EvaluationValue.Function trueFunc)
        {
            throw new InvalidCastException($"Expected arg 0 to be Function but got {trueValue}");
        }

        state = state.Pop(out var condition);
        if (condition is not EvaluationValue.Boolean conditionBool)
        {
            throw new InvalidCastException($"Expected arg 1 to be Boolean but got {condition}");
        }

        return conditionBool.Value ? evaluator.RunExpression(state, trueFunc.Body) : state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        var emitter = context.Emitter;
        
        stack = stack.Pop<CompilerType.Function>(out var trueFunc, out _);
        stack = stack.Pop<CompilerType.Boolean>(out var condition, out _);

        var mergeBlock = emitter.CreateBlockInCurrent("merge");
        var trueBlock = emitter.CreateBlockInCurrent("true");

        emitter.Branch(condition, trueBlock, mergeBlock);
        
        emitter.BeginBlock(trueBlock);
        var trueStack = ExpressionCompiler.CallFunction(emitter, stack, trueFunc);
        emitter.Branch(mergeBlock);
        
        emitter.BeginBlock(mergeBlock);
        
        return trueStack;
    }
}