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
        var sizeT = context.Handle.Int64Type;
        var stringT = LLVMTypeRef.CreatePointer(context.Handle.Int8Type, 0);

        Main = new NativeFunction(Array.Empty<LLVMTypeRef>(), int32, false);
        
        Sprintf = new NativeFunction(new[] { stringT, stringT }, int32, true);
        Strlen = new NativeFunction(new[] { stringT }, int64, false);
        Strcat = new NativeFunction(new[] { stringT, stringT }, stringT, false);
        Printf = new NativeFunction(new[] { stringT }, int32, true);
        
        Feof = new NativeFunction(new[] { dataPointer }, int32, false);
        Fgetc = new NativeFunction(new[] { dataPointer }, int32, false);
        Fputc = new NativeFunction(new[] { int32, dataPointer }, int32, false);
        Fgets = new NativeFunction(new[] { stringT, int32, dataPointer }, stringT, false);
        Fread = new NativeFunction(new[] { stringT, sizeT, sizeT, dataPointer }, sizeT, false);
        Fprintf = new NativeFunction(new[] { dataPointer, stringT }, int32, true);
        Fopen = new NativeFunction(new[] { stringT, stringT }, dataPointer, false);
        Fclose = new NativeFunction(new[] { dataPointer }, int32, false);
    }
    
    public NativeFunction Main { get; }
    
    public NativeFunction Sprintf { get; }
    public NativeFunction Strlen { get; }
    public NativeFunction Strcat { get; }
    public NativeFunction Printf { get; }
    
    public NativeFunction Feof { get; }
    public NativeFunction Fgetc { get; }
    public NativeFunction Fputc { get; }
    public NativeFunction Fgets { get; }
    public NativeFunction Fread { get; }
    public NativeFunction Fprintf { get; }
    public NativeFunction Fopen { get; }
    public NativeFunction Fclose { get; }
}