using LLVMSharp;
using L = LLVMSharp.LLVM;

namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    public CompilerValue Literal(string literal)
    {
        var value = L.BuildGlobalStringPtr(_builder, literal, "str");
        // TODO this should be copied from global -> gc heap
        return new CompilerValue(value, new CompilerType.String());
    }

    public CompilerValue Literal(long literal)
    {
        var value = L.ConstInt(LLVMTypeRef.Int64TypeInContext(_context), (ulong)literal, true);
        return new CompilerValue(value, new CompilerType.Long());
    }
}