using Stacky.Compilation;

namespace Stacky.Intrinsics.Arithmetic;

public class GreaterIntrinsic : BinaryComparisonIntrinsic
{
    public override string Name => ">";
    protected override bool Evaluate(long a, long b) => a > b;
    protected override CompilerValue Compile(CompilerFunctionContext context, CompilerValue a, CompilerValue b) => context.Emitter.Greater(a, b);
}