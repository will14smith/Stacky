using LLVMSharp;
using LLVMSharp.Interop;

namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    private CompilerLabel? _currentBlock;
    
    public CompilerLabel CreateBlock(CompilerValue functionRef, string name)
    {
        var block = BasicBlock.Create(_context, name, (Function) functionRef.Value);
        return new CompilerLabel(block);
    }    
    public CompilerLabel CreateBlockInCurrent(string name)
    {
        if (_currentBlock == null)
        {
            throw new Exception("not in a function currently");
        }

        var function = _currentBlock.Value.Block.Handle.Parent.AsFunction();
        var block = BasicBlock.Create(_context, name, function);
        
        return new CompilerLabel(block);
    }   
    
    public void BeginBlock(CompilerLabel label)
    {
        _currentBlock = label;
        _builder.SetInsertPoint(label.Block);
    }

    public CompilerValue DefineFunction(string name, CompilerType.Function type)
    {
        var functionType = LLVMTypeRef.CreateFunction(_context.Handle.VoidType, Array.Empty<LLVMTypeRef>());
        var functionRef = _module.AddFunction(name, functionType);
        functionRef.Linkage = LLVMLinkage.LLVMExternalLinkage;

        return new CompilerValue(functionRef.AsValue(), type);
    }

    public CompilerValue DefineNativeFunction(string name, NativeFunction type)
    {
        var existing = _module.GetNamedFunction(name);
        if (existing.Handle != IntPtr.Zero)
        {
            return new CompilerValue(existing.AsValue(), type);
        }
        
        var returnType = type.Output ?? _context.Handle.VoidType;
        var argTypes = type.Inputs.ToArray();

        var functionType = LLVMTypeRef.CreateFunction(returnType, argTypes, type.HasVarArgs);
        var functionRef = _module.AddFunction(name, functionType);
        functionRef.Linkage = LLVMLinkage.LLVMExternalLinkage;

        return new CompilerValue(functionRef.AsValue(), type);
    }

    public void VerifyFunction(CompilerValue functionRef)
    {
        functionRef.Value.Handle.VerifyFunction(LLVMVerifierFailureAction.LLVMPrintMessageAction);
    }
    
    public CompilerValue Call(CompilerValue target, CompilerType returnType, params CompilerValue[] args)
    {
        var value = _builder.CreateCall(target.Value, args.Select(x => x.Value).ToArray(), "result");
        
        return new CompilerValue(value, returnType);
    }

    public void CallVoid(CompilerValue function, params CompilerValue[] args) => _builder.CreateCall(function.Value, args.Select(x => x.Value).ToArray());

    public void Ret(CompilerValue value) => _builder.CreateRet(value.Value);
    public void RetVoid() => _builder.CreateRetVoid();
}