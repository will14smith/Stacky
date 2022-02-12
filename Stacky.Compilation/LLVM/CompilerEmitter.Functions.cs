using LLVMSharp;
using L = LLVMSharp.LLVM;

namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    public CompilerLabel BeginBlock(CompilerValue functionRef)
    {
        var block = L.AppendBasicBlockInContext(_context, functionRef.Value, "block");

        L.PositionBuilderAtEnd(_builder, block);

        return new CompilerLabel(block);
    }

    public CompilerValue DefineFunction(string name, CompilerType.Function type)
    {
        var functionType =
            LLVMTypeRef.FunctionType(LLVMTypeRef.VoidTypeInContext(_context), Array.Empty<LLVMTypeRef>(), false);

        var functionRef = L.AddFunction(_module, name, functionType);
        L.SetLinkage(functionRef, LLVMLinkage.LLVMExternalLinkage);

        return new CompilerValue(functionRef, type);
    }

    public CompilerValue DefineNativeFunction(string name, NativeFunction type)
    {
        var returnType = type.Output ?? LLVMTypeRef.VoidTypeInContext(_context);
        var argTypes = type.Inputs.ToArray();

        var functionType = LLVMTypeRef.FunctionType(returnType, argTypes, type.HasVarArgs);

        var functionRef = L.AddFunction(_module, name, functionType);
        L.SetLinkage(functionRef, LLVMLinkage.LLVMExternalLinkage);

        return new CompilerValue(functionRef, type);
    }

    public void VerifyFunction(CompilerValue functionRef)
    {
        L.VerifyFunction(functionRef.Value, LLVMVerifierFailureAction.LLVMPrintMessageAction);
    }
    
    public CompilerValue Call(CompilerValue target, CompilerType returnType, params CompilerValue[] args)
    {
        var llvmValue = L.BuildCall(_builder, target.Value, args.Select(x => x.Value).ToArray(), "result");

        return new CompilerValue(llvmValue, returnType);
    }

    public void CallVoid(CompilerValue function, params CompilerValue[] args) => L.BuildCall(_builder, function.Value, args.Select(x => x.Value).ToArray(), "");

    public void RetVoid() => L.BuildRetVoid(_builder);
}