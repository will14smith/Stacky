using LLVMSharp;
using LLVMSharp.Interop;

namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    public CompilerStruct DefineStruct(string name, CompilerType.Struct type)
    {
        var typeRef = _context.Handle.CreateNamedStruct(name);
        
        var fieldTypes = type.Fields.Select(f => _types.ToLLVM(f.Type)).ToArray();
        typeRef.StructSetBody(fieldTypes, true);
        
        return new CompilerStruct(typeRef, type);
    }

    public CompilerValue StructInit(CompilerValue target)
    {
        var zero = LLVMValueRef.CreateConstInt(_context.Handle.Int8Type, 0, false);

        _builder.CreateMemSet(target.Value, zero.AsValue(), target.Value.Handle.TypeOf.ElementType.SizeOf.AsValue(), 0);
        
        return target;
    }
    
    public CompilerValue FieldPointer(CompilerValue target, string fieldName)
    {
        var pointerType = (CompilerType.Pointer) target.Type;
        var structType = (CompilerType.Struct) pointerType.Type;

        var (field, fieldIndex) = structType.Fields.Select((x, i) => (Field: x, Index: i)).First(x => x.Field.Name == fieldName);

        var offset = _builder.CreateStructGEP(target.Value, (uint) fieldIndex, "field");
        
        return new CompilerValue(offset, new CompilerType.Pointer(field.Type));
    }
}