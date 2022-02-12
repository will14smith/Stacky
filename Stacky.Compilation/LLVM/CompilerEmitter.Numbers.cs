using L = LLVMSharp.LLVM;

namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    public CompilerValue Add(CompilerValue a, CompilerValue b)
    {
        var llvmValue = L.BuildAdd(_builder, a.Value, b.Value, "result");
        return new CompilerValue(llvmValue, new CompilerType.Long());
    }

    public CompilerValue Mul(CompilerValue a, CompilerValue b)
    {
        var llvmValue = L.BuildMul(_builder, a.Value, b.Value, "result");
        return new CompilerValue(llvmValue, new CompilerType.Long());
    }
}