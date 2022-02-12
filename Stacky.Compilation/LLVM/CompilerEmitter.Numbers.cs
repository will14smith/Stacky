using LLVMSharp;
using L = LLVMSharp.LLVM;

namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    // TODO handle different types (i.e. not only i64)

    public CompilerValue Add(CompilerValue a, CompilerValue b)
    {
        var llvmValue = L.BuildAdd(_builder, a.Value, b.Value, "result");
        return new CompilerValue(llvmValue, new CompilerType.Long());
    }
    
    public CompilerValue Sub(CompilerValue a, CompilerValue b)
    {
        var llvmValue = L.BuildSub(_builder, a.Value, b.Value, "result");
        return new CompilerValue(llvmValue, new CompilerType.Long());
    }

    public CompilerValue Mul(CompilerValue a, CompilerValue b)
    {
        var llvmValue = L.BuildMul(_builder, a.Value, b.Value, "result");
        return new CompilerValue(llvmValue, new CompilerType.Long());
    }
    
    public CompilerValue Greater(CompilerValue a, CompilerValue b)
    {
        var llvmValue = L.BuildICmp(_builder, LLVMIntPredicate.LLVMIntSGT, a.Value, b.Value, "result");
        return new CompilerValue(llvmValue, new CompilerType.Boolean());
    }
}