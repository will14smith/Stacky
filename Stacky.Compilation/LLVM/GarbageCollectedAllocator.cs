using LLVMSharp;
using LLVMSharp.Interop;

namespace Stacky.Compilation.LLVM;

public class GarbageCollectedAllocator
{
    private readonly LLVMContext _context;
    private readonly LLVMModuleRef _module;
    private readonly IRBuilder _builder;
    private readonly GarbageCollectedTypes _types;
    
    private readonly Value _gcRef;

    private readonly Value _gcNewFn;
    private readonly Value _gcDestroyFn;
    private readonly Value _gcAllocateFn;
    private readonly Value _gcAllocateRawFn;
    private readonly Value _gcRootAddFn;
    private readonly Value _gcRootRemoveFn;
    
    public GarbageCollectedAllocator(LLVMContext context, LLVMModuleRef module, IRBuilder builder)
    {
        _context = context;
        _module = module;
        _builder = builder;
        
        _types = new GarbageCollectedTypes(context, module, builder);

        var gcType = LLVMTypeRef.CreatePointer(_context.Handle.VoidType, 0);
        var gc = _module.AddGlobal(gcType, "gc");
        gc.Initializer = LLVMValueRef.CreateConstPointerNull(gcType);
        _gcRef = gc.AsValue();

        
        var dataPointerType = LLVMTypeRef.CreatePointer(_context.Handle.Int8Type, 0);
        // TODO...
        
        _gcNewFn = DefineExternFn("gc_new", gcType, Array.Empty<LLVMTypeRef>());
        _gcDestroyFn = DefineExternFn("gc_destroy", _context.Handle.VoidType, new [] { gcType });
        _gcAllocateFn = DefineExternFn("gc_allocate", dataPointerType, new [] { gcType, LLVMTypeRef.CreatePointer(_types.TypeDefType, 0) });
        _gcAllocateRawFn = DefineExternFn("gc_allocate_raw", dataPointerType, new [] { gcType, _context.Handle.Int64Type });
        _gcRootAddFn = DefineExternFn("gc_root_add", _context.Handle.VoidType, new [] { gcType, dataPointerType });
        _gcRootRemoveFn = DefineExternFn("gc_root_remove", _context.Handle.VoidType, new [] { gcType, dataPointerType });
    }

    private Value DefineExternFn(string name, LLVMTypeRef returnType, LLVMTypeRef[] parameterTypes)
    {
        var fnType = LLVMTypeRef.CreateFunction(returnType, parameterTypes);
        var fn = _module.AddFunction(name, fnType);
        fn.Linkage = LLVMLinkage.LLVMExternalLinkage;
        return fn.AsValue();
    }

    public void Init() => _builder.CreateStore(_builder.CreateCall(_gcNewFn), _gcRef);
    public void Destroy() => _builder.CreateCall(_gcDestroyFn, new Value[] { _builder.CreateLoad(_gcRef) });

    public CompilerValue Allocate(CompilerStruct type)
    {
        var gc = _builder.CreateLoad(_gcRef);
        var typeDef = _types.Get(type.Type);
        
        var value = _builder.CreateCall(_gcAllocateFn, new[] { gc, typeDef }, "allocated");
        
        var structDef = LLVMTypeRef.CreateStruct(type.TypeRef.StructElementTypes, type.TypeRef.IsPackedStruct);
        var typeRef = LLVMTypeRef.CreatePointer(structDef, 0).AsType();
        var cast = _builder.CreateCast(Instruction.CastOps.BitCast, value, typeRef, "struct");

        return new CompilerValue(cast, new CompilerType.Pointer(type.Type));
    }
    
    public CompilerValue AllocateRaw(CompilerType type, CompilerValue length)
    {
        var gc = _builder.CreateLoad(_gcRef);

        var value = _builder.CreateCall(_gcAllocateRawFn, new[] { gc, length.Value }, "allocated");

        return new CompilerValue(value, type);
    }

    public void RootAdd(CompilerValue value)
    {
        var gc = _builder.CreateLoad(_gcRef);
        _builder.CreateCall(_gcRootAddFn, new[] { gc, value.Value });
    }
    
    public void RootRemove(CompilerValue value)
    {
        var gc = _builder.CreateLoad(_gcRef);
        _builder.CreateCall(_gcRootRemoveFn, new[] { gc, value.Value });
    }
}