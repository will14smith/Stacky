using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Syntax;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Strings;

public class StringIndexGetter : IIntrinsic
{
    public string Name => "!";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        state = state.NewVariable(new StackySort.Numeric(), out var index);
        
        var str = new StackyType.String();
        var chr = new StackyType.Integer(false, SyntaxType.IntegerSize.S8);

        type = new StackyType.Function(
            StackyType.MakeComposite(stack, str, index), 
            StackyType.MakeComposite(stack, chr) 
        );
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<EvaluationValue.Int64>(out var index);
        state = state.Pop<EvaluationValue.String>(out var str);

        var chr = str.Value[(int)index.Value];
        
        state = state.Push(new EvaluationValue.Int64(chr));
        return state;
    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack) => throw new NotImplementedException();
}