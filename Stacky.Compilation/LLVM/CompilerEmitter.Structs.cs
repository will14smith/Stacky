using LLVMSharp.Interop;

namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    public CompilerStruct DefineStruct(string name, CompilerType.Struct type)
    {
        var typeRef = _context.Handle.CreateNamedStruct(name);
        
        var fieldTypes = type.Fields.Select(f => _types.ToLLVM(f.Type)).ToArray();
        typeRef.StructSetBody(fieldTypes, false);
        
        return new CompilerStruct(typeRef, type);
    }

    public CompilerValue StructInit(CompilerValue target)
    {
        var zero = LLVMValueRef.CreateConstInt(_context.Handle.Int8Type, 0, false);

        _builder.CreateMemSet(target.Value, zero.AsValue(), target.Value.Handle.TypeOf.SizeOf.AsValue(), 0);
        
        return target;
    }

    public CompilerValue StructSize(CompilerStruct type)
    {
        var size = type.TypeRef.SizeOf;
        // TODO is this a Long?
        return new CompilerValue(size.AsValue(), new CompilerType.Long());
    }
}