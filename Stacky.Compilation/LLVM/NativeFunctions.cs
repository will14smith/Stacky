using LLVMSharp;

namespace Stacky.Compilation.LLVM;

public class NativeFunctions
{
    public NativeFunctions(LLVMContextRef context)
    {
        var dataPointer = LLVMTypeRef.PointerType(LLVMTypeRef.Int8TypeInContext(context), 0);
        var int32 = LLVMTypeRef.Int32TypeInContext(context);
        var int64 = LLVMTypeRef.Int64TypeInContext(context);
        var stringT = LLVMTypeRef.PointerType(LLVMTypeRef.Int8TypeInContext(context), 0);
        var voidT = LLVMTypeRef.VoidTypeInContext(context);
        
        GcAllocateRaw = new NativeFunction(new[] { int64 }, dataPointer, false);
        GcRootAdd = new NativeFunction(new[] { dataPointer }, voidT, false);
        GcRootRemove = new NativeFunction(new[] { dataPointer }, voidT, false);
        
        Sprintf = new NativeFunction(new[] { stringT, stringT }, int32, true);
        Strlen = new NativeFunction(new[] { stringT }, int64, false);
        Strcat = new NativeFunction(new[] { stringT, stringT }, stringT, false);
        Printf = new NativeFunction(new[] { stringT }, int32, true);
    }

    public NativeFunction GcAllocateRaw { get; }
    public NativeFunction GcRootAdd { get; }
    public NativeFunction GcRootRemove { get; }
    
    public NativeFunction Sprintf { get; }
    public NativeFunction Strlen { get; }
    public NativeFunction Strcat { get; }
    public NativeFunction Printf { get; }
}