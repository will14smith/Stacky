using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Flow;

public class IfElseIntrinsic : IIntrinsic
{
    public string Name => "if-else";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var input);
        state = state.NewStackVariable(out var output);
        
        var condition = new StackyType.Boolean();
        var trueFunc = new StackyType.Function(input, output);
        var falseFunc = new StackyType.Function(input, output);

        type = new StackyType.Function(
            StackyType.MakeComposite(input, condition, trueFunc, falseFunc),
            output
        );
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop(out var falseValue);
        if (falseValue is not EvaluationValue.Closure falseFunc)
        {
            throw new InvalidCastException($"Expected arg 0 to be Function but got {falseValue}");
        }
        
        state = state.Pop(out var trueValue);
        if (trueValue is not EvaluationValue.Closure trueFunc)
        {
            throw new InvalidCastException($"Expected arg 1 to be Function but got {trueValue}");
        }

        state = state.Pop(out var condition);
        if (condition is not EvaluationValue.Boolean conditionBool)
        {
            throw new InvalidCastException($"Expected arg 2 to be Boolean but got {condition}");
        }

        return evaluator.RunClosure(state, conditionBool.Value ? trueFunc : falseFunc);
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        var emitter = context.Emitter;
        
        stack = stack.Pop(out var falseClosure, out var removeFalseClosureRoot);
        stack = stack.Pop(out var trueClosure, out var removeTrueClosureRoot);
        stack = stack.Pop<CompilerType.Boolean>(out var condition, out _);

        var mergeBlock = emitter.CreateBlockInCurrent("merge");
        var trueBlock = emitter.CreateBlockInCurrent("true");
        var falseBlock = emitter.CreateBlockInCurrent("false");

        emitter.Branch(condition, trueBlock, falseBlock);
        
        emitter.BeginBlock(trueBlock);
        var trueStack = context.Invoke(stack, trueClosure);
        emitter.Branch(mergeBlock);
        
        emitter.BeginBlock(falseBlock);
        // the stacks will have the same output so we don't need this one
        _ = context.Invoke(stack, falseClosure);
        emitter.Branch(mergeBlock);
        
        emitter.BeginBlock(mergeBlock);

        removeFalseClosureRoot();
        removeTrueClosureRoot();
        
        return trueStack;
    }
}