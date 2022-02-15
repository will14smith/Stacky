using LLVMSharp;
using LLVMSharp.Interop;

namespace Stacky.Compilation.LLVM;

internal class LLVMTypeBuilder
{
    private readonly LLVMContext _context;

    public LLVMTypeBuilder(LLVMContext context)
    {
        _context = context;
    }

    public LLVMTypeRef ToLLVM(CompilerType type)
    {
        return type switch
        {
            // functions get values from the value stack rather than through arguments/returns
            CompilerType.Function => LLVMTypeRef.CreatePointer(LLVMTypeRef.CreateFunction(_context.Handle.VoidType, Array.Empty<LLVMTypeRef>(), false), 0),
            CompilerType.Boolean => _context.Handle.Int1Type,
            CompilerType.Long => _context.Handle.Int64Type,
            CompilerType.String => LLVMTypeRef.CreatePointer(_context.Handle.Int8Type, 0),
            
            CompilerType.Struct def => LLVMTypeRef.CreatePointer(LLVMTypeRef.CreateStruct(def.Fields.Select(f => ToLLVM(f.Type)).ToArray(), false), 0),
            
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}