﻿using Stacky.Compilation;
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
        if (loopBody is not EvaluationValue.Function loopBodyFunc)
        {
            throw new InvalidCastException($"Expected arg 0 to be Function but got {loopBody}");
        }
        
        state = state.Pop(out var conditionBody);
        if (conditionBody is not EvaluationValue.Function conditionBodyFunc)
        {
            throw new InvalidCastException($"Expected arg 1 to be Function but got {conditionBody}");
        }

        while (true)
        {
            state = evaluator.RunExpression(state, conditionBodyFunc.Body);
            
            state = state.Pop(out var condition);
            if (condition is not EvaluationValue.Boolean conditionBool)
            {
                throw new InvalidCastException($"Expected arg 2 to be Boolean but got {condition}");
            }

            if (!conditionBool.Value)
            {
                break;
            }

            state = evaluator.RunExpression(state, loopBodyFunc.Body);
        }

        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        var emitter = context.Emitter;
        
        stack = stack.Pop<CompilerType.Function>(out var loopFunc, out _);
        stack = stack.Pop<CompilerType.Function>(out var condFunc, out _);

        var doneBlock = emitter.CreateBlockInCurrent("done");
        var condBlock = emitter.CreateBlockInCurrent("cond");
        var loopBlock = emitter.CreateBlockInCurrent("loop");

        emitter.Branch(condBlock);
        
        emitter.BeginBlock(loopBlock);
        // this stack isn't needed because it won't have changed
        _ = ExpressionCompiler.CallFunction(emitter, stack, loopFunc);
        emitter.Branch(condBlock);
        
        emitter.BeginBlock(condBlock);
        var condStack = ExpressionCompiler.CallFunction(emitter, stack, condFunc);
        // this stack isn't needed because it won't have changed
        _ = condStack.Pop<CompilerType.Boolean>(out var condition, out _);
        emitter.Branch(condition, loopBlock, doneBlock);

        emitter.BeginBlock(doneBlock);

        return stack;
    }
}