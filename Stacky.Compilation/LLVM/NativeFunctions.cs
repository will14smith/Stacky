using LLVMSharp;
using LLVMSharp.Interop;

namespace Stacky.Compilation.LLVM;

public class NativeFunctions
{
    public NativeFunctions(LLVMContext context)
    {
        
        var dataPointer = LLVMTypeRef.CreatePointer(context.Handle.Int8Type, 0);
        var int32 = context.Handle.Int32Type;
        var int64 = context.Handle.Int64Type;
        var stringT = LLVMTypeRef.CreatePointer(context.Handle.Int8Type, 0);
        var voidT = context.Handle.VoidType;
        
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