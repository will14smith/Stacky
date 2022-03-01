using LLVMSharp;
using LLVMSharp.Interop;

namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    private readonly Value _stackPointer;

    private Value CreateStack()
    {
        var stackType = LLVMTypeRef.CreateArray(_context.Handle.Int8Type, 16 * 1024);
        var stack = _module.AddGlobal(stackType, "stackRoot");
        stack.Initializer = LLVMValueRef.CreateConstNull(stackType);
        
        var stackPointerType = LLVMTypeRef.CreatePointer(_context.Handle.Int8Type, 0);
        var stackPointer = _module.AddGlobal(stackPointerType, "stackPointer");
        stackPointer.Initializer = stack;

        return stackPointer.AsValue();
    }
    
    public void Push(CompilerValue value)
    {
        var llvmValue = value.Value;
        var llvmType = llvmValue.Handle.TypeOf;

        if (!_types.IsCompatible(value.Type, llvmType))
        {
            throw new InvalidOperationException($"Pushing value to stack with incorrect type: compiler type = {value.Type}, compiled type = {llvmType}");
        }
        
        var stack = _builder.CreateLoad(_stackPointer, "sp");
        var stackTyped = _builder.CreatePointerCast(stack, LLVMTypeRef.CreatePointer(llvmType, 0).AsType(), "spTyped");
        _builder.CreateStore(llvmValue, stackTyped);
        
        var newStack = _builder.CreateGEP(stack, new[] { llvmType.SizeOf.AsValue() }.AsSpan(), "sp");
        _builder.CreateStore(newStack, _stackPointer);
    }

    public CompilerValue Pop(CompilerType type)
    {
        var (value, newStack) = PeekInternal(type);
        
        _builder.CreateStore(newStack, _stackPointer);

        return value;
    }

    public CompilerValue Peek(params CompilerType[] types) => PeekInternal(types).Value;

    private (CompilerValue Value, Value NewStackPointer) PeekInternal(params CompilerType[] types)
    {
        var llvmTypes = types.Select(t => _types.ToLLVM(t)).ToArray();
       
        var peekType = types[^1];
        var llvmPeekType = llvmTypes[^1];

        var size = llvmTypes.Select(x => x.SizeOf).Aggregate(LLVMValueRef.CreateConstAdd);
        
        var stack = _builder.CreateLoad(_stackPointer, "sp");
        var newStack = _builder.CreateGEP(stack, new[] { LLVMValueRef.CreateConstNeg(size).AsValue() }, "sp");

        var stackTyped = _builder.CreatePointerCast(newStack, LLVMTypeRef.CreatePointer(llvmPeekType, 0).AsType(), "spTyped");
        var llvmValue = _builder.CreateLoad(stackTyped, "value");

        var value = new CompilerValue(llvmValue, peekType);

        return (value, newStack);
    } 
}