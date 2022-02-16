using Stacky.Compilation;

namespace Stacky.Intrinsics.Arithmetic;

public class AddIntrinsic : BinaryArithmeticIntrinsic
{
    public override string Name => "+";

    protected override long Evaluate(long a, long b) => a + b;
    protected override CompilerValue Compile(CompilerFunctionContext context, CompilerValue a, CompilerValue b) => context.Emitter.Add(a, b);
}