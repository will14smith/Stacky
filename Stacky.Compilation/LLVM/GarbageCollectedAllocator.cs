using LLVMSharp;
using LLVMSharp.Interop;

namespace Stacky.Compilation.LLVM;

public class GarbageCollectedAllocator
{
    private LLVMModuleRef _module;
    private readonly IRBuilder _builder;
    private readonly CompilerEmitter _emitter;
    
    private readonly GarbageCollectedTypes _types;
    
    private readonly Value _gcRef;

    private readonly Value _gcNewFn;
    private readonly Value _gcDestroyFn;
    private readonly Value _gcAllocateFn;
    private readonly Value _gcAllocateRawFn;
    private readonly Value _gcRootAddFn;
    private readonly Value _gcRootRemoveFn;
    private readonly Value _gcCollectFn;
    private readonly Value _gcStatsFn;
    private readonly Value _gcDumpFn;
    
    public GarbageCollectedAllocator(LLVMContext context, LLVMModuleRef module, IRBuilder builder, CompilerEmitter emitter)
    {
        _module = module;
        _builder = builder;
        _emitter = emitter;

        _types = new GarbageCollectedTypes(context, module, builder);

        var gcType = LLVMTypeRef.CreatePointer(context.Handle.VoidType, 0);
        var gc = _module.AddGlobal(gcType, "gc");
        gc.Initializer = LLVMValueRef.CreateConstPointerNull(gcType);
        _gcRef = gc.AsValue();

        var statsType = LLVMTypeRef.CreateStruct(new[]
        {
            LLVMTypeRef.Int64,
            LLVMTypeRef.Int64,
            LLVMTypeRef.Int64,
            LLVMTypeRef.Int64,
        }, false);
        
        var dataPointerType = LLVMTypeRef.CreatePointer(context.Handle.Int8Type, 0);
        
        _gcNewFn = DefineExternFn("gc_new", gcType, Array.Empty<LLVMTypeRef>());
        _gcDestroyFn = DefineExternFn("gc_destroy", context.Handle.VoidType, new [] { gcType });
        _gcAllocateFn = DefineExternFn("gc_allocate", dataPointerType, new [] { gcType, LLVMTypeRef.CreatePointer(_types.TypeDefType, 0) });
        _gcAllocateRawFn = DefineExternFn("gc_allocate_raw", dataPointerType, new [] { gcType, context.Handle.Int64Type });
        _gcRootAddFn = DefineExternFn("gc_root_add", context.Handle.VoidType, new [] { gcType, dataPointerType });
        _gcRootRemoveFn = DefineExternFn("gc_root_remove", context.Handle.VoidType, new [] { gcType, dataPointerType });
        _gcCollectFn = DefineExternFn("gc_collect", context.Handle.VoidType, new [] { gcType });
        _gcStatsFn = DefineExternFn("gc_stats", statsType, new [] { gcType });
        _gcDumpFn = DefineExternFn("gc_dump", context.Handle.VoidType, new [] { gcType });
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

    private Value? _statsFormat;
    
    public void Collect()
    {
        var gc = _builder.CreateLoad(_gcRef);
        _builder.CreateCall(_gcCollectFn, new Value[] { gc });
    }    
    
    public void PrintStats()
    {
        var gc = _builder.CreateLoad(_gcRef);
        var stats = _builder.CreateCall(_gcStatsFn, new Value[] { gc }, "stats");

        var alloca = _builder.CreateAlloca(stats.Handle.TypeOf.AsType());
        _builder.CreateStore(stats, alloca);
        
        var allocatedItems = _builder.CreateLoad(_builder.CreateStructGEP(alloca, 0, "allocated_items"));
        var allocatedItemSize = _builder.CreateLoad(_builder.CreateStructGEP(alloca, 1, "allocated_items_size"));
        var rootedItems = _builder.CreateLoad(_builder.CreateStructGEP(alloca, 2, "rooted_items"));
        var reachableItems = _builder.CreateLoad(_builder.CreateStructGEP(alloca, 3, "reachable_items"));

        _statsFormat ??= _emitter.Literal("allocated#=%ld allocated=%ld rooted#=%ld reachable#=%ld\n").Value;

        var printf = _emitter.DefineNativeFunction("printf", _emitter.NativeFunctions.Printf);
        _builder.CreateCall(printf.Value, new[] { _statsFormat, allocatedItems, allocatedItemSize, rootedItems, reachableItems });
    }

    public void Dump()
    {
        var gc = _builder.CreateLoad(_gcRef);
        _builder.CreateCall(_gcDumpFn, new Value[] { gc });
    }
}