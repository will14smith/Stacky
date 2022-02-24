using LLVMSharp;
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
    
    public CompilerValue StructCast(CompilerValue value, CompilerStruct type)
    {
        var typeRef = LLVMTypeRef.CreatePointer(type.TypeRef, 0).AsType();
        var cast = _builder.CreateCast(Instruction.CastOps.BitCast, value.Value, typeRef, "struct");

        return new CompilerValue(cast, type.Type);
    }

    public CompilerValue FieldPointer(CompilerValue target, string fieldName)
    {
        var structType = (CompilerType.Struct) target.Type;
        var (field, fieldIndex) = structType.Fields.Select((x, i) => (Field: x, Index: i)).First(x => x.Field.Name == fieldName);

        var offset = _builder.CreateStructGEP(target.Value, (uint) fieldIndex, "field");
        
        return new CompilerValue(offset, new CompilerType.Pointer(field.Type));
    }
}