using LLVMSharp;
using LLVMSharp.Interop;

namespace Stacky.Compilation.LLVM;

public struct CompilerStruct
{
    internal CompilerStruct(LLVMTypeRef typeRef, CompilerType type)
    {
        TypeRef = typeRef;
        Type = type;
    }

    internal LLVMTypeRef TypeRef { get; }
    public CompilerType Type { get; }
}