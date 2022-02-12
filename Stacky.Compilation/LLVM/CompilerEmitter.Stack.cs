using LLVMSharp;
using L = LLVMSharp.LLVM;

namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    private readonly LLVMValueRef _stackPointer;

    private LLVMValueRef CreateStack()
    {
        var stackType = LLVMTypeRef.ArrayType(LLVMTypeRef.Int8TypeInContext(_context), 16 * 1024);
        var stack = L.AddGlobal(_module, stackType, "stackRoot");
        stack.SetInitializer(L.ConstNull(stackType));
        
        var stackPointerType = LLVMTypeRef.PointerType(LLVMTypeRef.Int8TypeInContext(_context), 0);
        var stackPointer = L.AddGlobal(_module, stackPointerType, "stackPointer");
        stackPointer.SetInitializer(stack);

        return stackPointer;
    }
    
    public void Push(CompilerValue value)
    {
        var llvmValue = value.Value;

        var stack = L.BuildLoad(_builder, _stackPointer, "sp");
        var stackTyped = L.BuildPointerCast(_builder, stack, LLVMTypeRef.PointerType(llvmValue.TypeOf(), 0), "spTyped");
        L.BuildStore(_builder, llvmValue, stackTyped);

        var newStack = L.BuildGEP(_builder, stack, new[] { L.SizeOf(llvmValue.TypeOf()) }, "sp");
        L.BuildStore(_builder, newStack, _stackPointer);
    }

    public CompilerValue Pop(CompilerType type)
    {
        var llvmType = _types.ToLLVM(type);
        
        var stack = L.BuildLoad(_builder, _stackPointer, "sp");

        var newStack = L.BuildGEP(_builder, stack, new[] { L.ConstNeg(L.SizeOf(llvmType)) }, "sp");
        L.BuildStore(_builder, newStack, _stackPointer);

        var stackTyped = L.BuildPointerCast(_builder, newStack, LLVMTypeRef.PointerType(llvmType, 0), "spTyped");
        var llvmValue = L.BuildLoad(_builder, stackTyped, "value");

        return new CompilerValue(llvmValue, type);
    }
}