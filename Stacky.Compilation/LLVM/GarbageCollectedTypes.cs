using LLVMSharp;
using LLVMSharp.Interop;

namespace Stacky.Compilation.LLVM;

public class GarbageCollectedTypes
{
    private readonly LLVMContext _context;
    private LLVMModuleRef _module;
    private readonly IRBuilder _builder;

    public LLVMTypeRef TypeDefType { get; }
    private readonly LLVMTypeRef _fieldDefType;

    private readonly Lazy<Value> _u8;
    private readonly Lazy<Value> _i64;
    private readonly Lazy<Value> _ptr;
    
    private readonly Dictionary<string, Value> _typeDefs = new();

    public GarbageCollectedTypes(LLVMContext context, LLVMModuleRef module, IRBuilder builder)
    {
        _context = context;
        _module = module;
        _builder = builder;

        // TODO
        TypeDefType = _context.Handle.Int1Type;
        _fieldDefType = _context.Handle.Int1Type;

        _u8 = CreatePrimitive("u8", 1);
        _i64 = CreatePrimitive("i64", 8);
        _ptr = CreatePrimitive("ptr", 8);
    }
    
    public Value Get(CompilerType type)
    {
        return type switch
        {
            CompilerType.Byte => _u8.Value,
            CompilerType.Long => _i64.Value,
            CompilerType.Function => _ptr.Value,
            CompilerType.Pointer { Type: CompilerType.Struct @struct } => GetOrCreate(@struct),
            CompilerType.Pointer => _ptr.Value,
            CompilerType.String => _ptr.Value,
            
            CompilerType.Struct @struct => GetOrCreate(@struct),
         
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
    
    private Lazy<Value> CreatePrimitive(string name, ulong size)
    {
        return new Lazy<Value>(() =>
        {
            var typeDefRef = _module.AddGlobal(TypeDefType, $"type__{name}");
            typeDefRef.Initializer = LLVMValueRef.CreateConstStruct(
                new[]
                {
                    _builder.Handle.BuildGlobalString(name, $"type_name__{name}"),
                    LLVMValueRef.CreateConstInt(_context.Handle.Int32Type, 0, true),
                    LLVMValueRef.CreateConstInt(_context.Handle.Int64Type, size),
                },
                false
            );

            return typeDefRef.AsValue();
        });
    }
    
    private Value GetOrCreate(CompilerType.Struct type)
    {
        if (_typeDefs.TryGetValue(type.Name, out var typeDef))
        {
            return typeDef;
        }

        var typeDefRef = _module.AddGlobal(TypeDefType, $"type_def__{type.Name}");
        typeDef = typeDefRef.AsValue();
        _typeDefs.Add(type.Name, typeDef);

        
        var fields = new LLVMValueRef[type.Fields.Count + 1];
        for (var i = 0; i < type.Fields.Count; i++)
        {
            var field = type.Fields[i];
            
            var fieldRef = _module.AddGlobal(_fieldDefType, $"type_def_field_{i}__{type.Name}");
            fieldRef.Initializer = LLVMValueRef.CreateConstStruct(new LLVMValueRef[]
            {
                _builder.Handle.BuildGlobalString(field.Name),
                Get(field.Type).Handle,
            }, false);
            
            fields[i] = fieldRef;
        }

        var fieldPtrType = LLVMTypeRef.CreatePointer(_fieldDefType, 0);
        fields[type.Fields.Count] = LLVMValueRef.CreateConstNull(fieldPtrType);
        var fieldsRef = _module.AddGlobal(fieldPtrType, $"type_def_fields__{type.Name}");
        fieldsRef.Initializer = LLVMValueRef.CreateConstArray(fieldPtrType, fields);
        
        typeDefRef.Initializer = LLVMValueRef.CreateConstStruct(
            new[]
            {
                _builder.Handle.BuildGlobalString(type.Name, $"type_def_name__{type.Name}"),
                LLVMValueRef.CreateConstInt(_context.Handle.Int32Type, 1, true), 
                fieldsRef, 
            },
            false
        );

        return typeDef;
    }
}