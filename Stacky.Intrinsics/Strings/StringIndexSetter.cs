using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Syntax;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Strings;

public class StringIndexSetter : IIntrinsic
{
    public string Name => "!~";

    public InferenceState Infer(InferenceState state, out StackyType type)
    {
        state = state.NewStackVariable(out var stack);
        state = state.NewVariable(new StackySort.Numeric(), out var index);
        
        var str = new StackyType.String();
        var chr = new StackyType.Integer(false, SyntaxType.IntegerSize.S8);

        type = new StackyType.Function(
            StackyType.MakeComposite(stack, str, chr, index), 
            StackyType.MakeComposite(stack, str) 
        );
        return state;
    }

    public EvaluationState Evaluate(Evaluator evaluator, EvaluationState state)
    {
        state = state.Pop<EvaluationValue.Int64>(out var index);
        // TODO u8
        state = state.Pop<EvaluationValue.Int64>(out var chr);
        state = state.Pop<EvaluationValue.String>(out var str);

        str.Value[index.Value] = (byte)chr.Value;
        
        state = state.Push(str);
        return state;

    }

    public CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack)
    {
        var emitter = context.Emitter;
        
        stack = stack.Pop<CompilerType.Long>(out var index, out _);
        stack = stack.Pop<CompilerType.Byte>(out var chr, out _);
        stack = stack.Pop<CompilerType.String>(out var str, out var removeRoot);
        
        emitter.StoreIndex(str, index, chr);
        stack = stack.Push(str);
        
        removeRoot();

        return stack;
    }}