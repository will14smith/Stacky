using System.Reflection;
using LLVMSharp;
using LLVMSharp.Interop;

namespace Stacky.Compilation.LLVM;

public static class LLVMWorkarounds
{
    private delegate Value CreateValueDelegate(LLVMValueRef valueRef);
    private static readonly CreateValueDelegate CreateValue = (CreateValueDelegate) Delegate.CreateDelegate(typeof(CreateValueDelegate), typeof(Value).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic)!);

    private delegate LLVMSharp.Type CreateTypeDelegate(LLVMTypeRef typeRef);
    private static readonly CreateTypeDelegate CreateType = (CreateTypeDelegate) Delegate.CreateDelegate(typeof(CreateTypeDelegate), typeof(LLVMSharp.Type).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic)!);
    
    public static Value AsValue(this LLVMValueRef valueRef) => CreateValue(valueRef);
    public static Function AsFunction(this LLVMValueRef valueRef) => (Function) valueRef.AsValue();
    
    public static LLVMSharp.Type AsType(this LLVMTypeRef typeRef) => CreateType(typeRef);
}