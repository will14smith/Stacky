using LLVMSharp;
using LLVMSharp.Interop;

namespace Stacky.Compilation.LLVM;

public struct CompilerStruct
{
    internal CompilerStruct(LLVMTypeRef typeRef, CompilerType.Struct type)
    {
        TypeRef = typeRef;
        Type = type;
    }

    internal LLVMTypeRef TypeRef { get; }
    public CompilerType.Struct Type { get; }
}