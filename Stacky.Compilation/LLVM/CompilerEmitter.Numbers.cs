using LLVMSharp;
using LLVMSharp.Interop;

namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    public CompilerValue Add(CompilerValue a, CompilerValue b) => GetNumericResult(_builder.CreateAdd(a.Value, b.Value, "result"));
    public CompilerValue Sub(CompilerValue a, CompilerValue b) => GetNumericResult(_builder.CreateSub(a.Value, b.Value, "result"));
    public CompilerValue Mul(CompilerValue a, CompilerValue b) => GetNumericResult(_builder.CreateMul(a.Value, b.Value, "result"));

    private CompilerValue GetNumericResult(Value value)
    {
        var type = value.Handle.TypeOf switch
        {
            { Kind: LLVMTypeKind.LLVMIntegerTypeKind, IntWidth: 8 } => (CompilerType) new CompilerType.Byte(),
            { Kind: LLVMTypeKind.LLVMIntegerTypeKind, IntWidth: 32 } => new CompilerType.Int(),
            { Kind: LLVMTypeKind.LLVMIntegerTypeKind, IntWidth: 64 } => new CompilerType.Long(),
            
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
        
        return new CompilerValue(value, type);
    }
    
    public CompilerValue Greater(CompilerValue a, CompilerValue b)
    {
        var value = _builder.CreateICmp(CmpInst.Predicate.ICMP_SGT, a.Value, b.Value, "result");
        return new CompilerValue(value, new CompilerType.Boolean());
    }
    public CompilerValue GreaterOrEqual(CompilerValue a, CompilerValue b)
    {
        var value = _builder.CreateICmp(CmpInst.Predicate.ICMP_SGE, a.Value, b.Value, "result");
        return new CompilerValue(value, new CompilerType.Boolean());
    }
    public CompilerValue Lesser(CompilerValue a, CompilerValue b)
    {
        var value = _builder.CreateICmp(CmpInst.Predicate.ICMP_SLT, a.Value, b.Value, "result");
        return new CompilerValue(value, new CompilerType.Boolean());
    }
    public CompilerValue Equal(CompilerValue a, CompilerValue b)
    {
        var value = _builder.CreateICmp(CmpInst.Predicate.ICMP_EQ, a.Value, b.Value, "result");
        return new CompilerValue(value, new CompilerType.Boolean());
    }   
    public CompilerValue NotEqual(CompilerValue a, CompilerValue b)
    {
        var value = _builder.CreateICmp(CmpInst.Predicate.ICMP_NE, a.Value, b.Value, "result");
        return new CompilerValue(value, new CompilerType.Boolean());
    }

    public CompilerValue Not(CompilerValue a)
    {
        var value = _builder.CreateNot(a.Value, "result");

        return new CompilerValue(value, new CompilerType.Boolean());
    }


}