using LLVMSharp;

namespace Stacky.Language.Compilation.LLVM;

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
            CompilerType.Function function => throw new NotImplementedException(),
            CompilerType.Long l => LLVMTypeRef.Int64TypeInContext(_context),
            CompilerType.String s => LLVMTypeRef.PointerType(LLVMTypeRef.Int8TypeInContext(_context), 0),
            
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}