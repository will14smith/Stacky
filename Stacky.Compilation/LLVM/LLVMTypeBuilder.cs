using LLVMSharp;

namespace Stacky.Compilation.LLVM;

internal class LLVMTypeBuilder
{
    private readonly LLVMContextRef _context;

    public LLVMTypeBuilder(LLVMContextRef context)
    {
        _context = context;
    }

    public LLVMTypeRef ToLLVM(CompilerType type)
    {
        return type switch
        {
            // functions get values from the value stack rather than through arguments/returns
            CompilerType.Function => LLVMTypeRef.PointerType(LLVMTypeRef.FunctionType(LLVMTypeRef.VoidTypeInContext(_context), Array.Empty<LLVMTypeRef>(), false), 0),
            CompilerType.Boolean => LLVMTypeRef.Int1Type(),
            CompilerType.Long => LLVMTypeRef.Int64TypeInContext(_context),
            CompilerType.String => LLVMTypeRef.PointerType(LLVMTypeRef.Int8TypeInContext(_context), 0),
            
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}