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
            CompilerType.Byte => _context.Handle.Int8Type,
            CompilerType.Int => _context.Handle.Int32Type,
            CompilerType.Long => _context.Handle.Int64Type,
            CompilerType.String => LLVMTypeRef.CreatePointer(_context.Handle.Int8Type, 0),
            
            CompilerType.Struct def => LLVMTypeRef.CreateStruct(def.Fields.Select(f => ToLLVM(f.Type)).ToArray(), true),
            CompilerType.Pointer ptr => LLVMTypeRef.CreatePointer(ToLLVM(ptr.Type), 0),
            
            ILLVMTypeConversion conversion => conversion.ToLLVM(_context),
            
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public bool IsCompatible(CompilerType compilerType, LLVMTypeRef llvmType)
    {
        return compilerType switch
        {
            CompilerType.Boolean => llvmType.Kind == LLVMTypeKind.LLVMIntegerTypeKind && llvmType.IntWidth == 1,
            CompilerType.Byte => llvmType.Kind == LLVMTypeKind.LLVMIntegerTypeKind && llvmType.IntWidth == 8,
            CompilerType.Int => llvmType.Kind == LLVMTypeKind.LLVMIntegerTypeKind && llvmType.IntWidth == 32,
            CompilerType.Long => llvmType.Kind == LLVMTypeKind.LLVMIntegerTypeKind && llvmType.IntWidth == 64,
            CompilerType.String => llvmType.Kind == LLVMTypeKind.LLVMPointerTypeKind && llvmType.ElementType.Kind == LLVMTypeKind.LLVMIntegerTypeKind && llvmType.ElementType.IntWidth == 8,
            
            CompilerType.Pointer ptr => llvmType.Kind == LLVMTypeKind.LLVMPointerTypeKind && IsCompatible(ptr.Type, llvmType.ElementType),
            // TODO check structure is compatible
            CompilerType.Struct => llvmType.Kind == LLVMTypeKind.LLVMStructTypeKind,
            
            ILLVMTypeConversion conversion => conversion.IsCompatible(llvmType),

            _ => throw new ArgumentOutOfRangeException(nameof(compilerType))
        };
    }
}

public interface ILLVMTypeConversion
{
    LLVMTypeRef ToLLVM(LLVMContext context);
    bool IsCompatible(LLVMTypeRef llvmType);
}