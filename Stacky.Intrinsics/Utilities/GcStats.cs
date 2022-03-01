using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Utilities;

public class GcStats : IIntrinsic
{
    public string Name => "gc-stats";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);

        type = new StackyType.Function(stack, stack);
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        // nop
        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        context.Emitter.GC.PrintStats();
        return stack;
    }
}

public class GcDump : IIntrinsic
{
    public string Name => "gc-dump";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);

        type = new StackyType.Function(stack, stack);
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        // nop
        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        context.Emitter.GC.Dump();
        return stack;
    }
}