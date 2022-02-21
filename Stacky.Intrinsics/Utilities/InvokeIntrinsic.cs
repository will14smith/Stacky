﻿using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Utilities;

public class InvokeIntrinsic : IIntrinsic
{
    public string Name => "invoke";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var inStack);
        state = state.NewStackVariable(out var outStack);

        var fnType = new StackyType.Function(inStack, outStack);
        
        type = new StackyType.Function(StackyType.MakeComposite(inStack, fnType), outStack);
        
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop(out var a);

        if (a is not EvaluationValue.Function function)
        {
            throw new InvalidCastException($"Expected arg 0 to be Function but got {a}");
        }

        return evaluator.RunExpression(state, function.Body);
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        stack = stack.Pop<CompilerType.Function>(out var value, out _);

        return ExpressionCompiler.CallFunction(context.Emitter, stack, value);
    }
}