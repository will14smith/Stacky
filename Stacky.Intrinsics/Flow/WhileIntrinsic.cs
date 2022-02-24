using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Flow;

public class WhileIntrinsic : IIntrinsic
{
    public string Name => "while";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack1);
        state = state.NewStackVariable(out var stack2);
        
        var condition = new StackyType.Boolean();
        var conditionFunc = new StackyType.Function(stack1, StackyType.MakeComposite(stack2, condition));
        var loopFunc = new StackyType.Function(stack2, stack1);

        type = new StackyType.Function(
            StackyType.MakeComposite(stack1, conditionFunc, loopFunc),
            stack1
        );
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop(out var loopBody);
        if (loopBody is not EvaluationValue.Closure loopBodyFunc)
        {
            throw new InvalidCastException($"Expected arg 0 to be Function but got {loopBody}");
        }
        
        state = state.Pop(out var conditionBody);
        if (conditionBody is not EvaluationValue.Closure conditionBodyFunc)
        {
            throw new InvalidCastException($"Expected arg 1 to be Function but got {conditionBody}");
        }

        while (true)
        {
            state = evaluator.RunClosure(state, conditionBodyFunc);
            
            state = state.Pop(out var condition);
            if (condition is not EvaluationValue.Boolean conditionBool)
            {
                throw new InvalidCastException($"Expected arg 2 to be Boolean but got {condition}");
            }

            if (!conditionBool.Value)
            {
                break;
            }

            state = evaluator.RunClosure(state, loopBodyFunc);
        }

        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        var emitter = context.Emitter;
        
        stack = stack.Pop(out var loopClosure, out var removeLoopClosureRoot);
        stack = stack.Pop(out var conditionClosure, out var removeConditionClosureRoot);

        var doneBlock = emitter.CreateBlockInCurrent("done");
        var condBlock = emitter.CreateBlockInCurrent("cond");
        var loopBlock = emitter.CreateBlockInCurrent("loop");

        emitter.Branch(condBlock);
        
        emitter.BeginBlock(loopBlock);
        // this stack isn't needed because it won't have changed
        _ = context.Invoke(stack, loopClosure);
        emitter.Branch(condBlock);
        
        emitter.BeginBlock(condBlock);
        var condStack = context.Invoke(stack, conditionClosure);
        // this stack isn't needed because it won't have changed
        _ = condStack.Pop<CompilerType.Boolean>(out var condition, out _);
        emitter.Branch(condition, loopBlock, doneBlock);

        emitter.BeginBlock(doneBlock);

        removeLoopClosureRoot();
        removeConditionClosureRoot();
        
        return stack;
    }
}