using LLVMSharp;
using LLVMSharp.Interop;
using Stacky.Compilation;
using Stacky.Compilation.LLVM;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics.Io;

public record FileInferenceType : StackyType, ICompilerTypeConversion
{
    public CompilerType ToCompilerType()
    {
        return new FileCompilerType();
    }
}

public record FileCompilerType : CompilerType, ILLVMTypeConversion
{
    public LLVMTypeRef ToLLVM(LLVMContext context)
    {
        return LLVMTypeRef.CreatePointer(context.Handle.Int8Type, 0);
    }
}