using LLVMSharp;

namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    // TODO handle different types (i.e. not only i64)

    public CompilerValue Add(CompilerValue a, CompilerValue b)
    {
        var value = _builder.CreateAdd(a.Value, b.Value, "result");
        return new CompilerValue(value, new CompilerType.Long());
    }
    
    public CompilerValue Sub(CompilerValue a, CompilerValue b)
    {
        var value = _builder.CreateSub(a.Value, b.Value, "result");
        return new CompilerValue(value, new CompilerType.Long());
    }

    public CompilerValue Mul(CompilerValue a, CompilerValue b)
    {
        var value = _builder.CreateMul(a.Value, b.Value, "result");
        return new CompilerValue(value, new CompilerType.Long());
    }
    
    public CompilerValue Greater(CompilerValue a, CompilerValue b)
    {
        var value = _builder.CreateICmp(CmpInst.Predicate.ICMP_SGT, a.Value, b.Value, "result");
        return new CompilerValue(value, new CompilerType.Long());
    }
}