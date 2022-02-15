using LLVMSharp.Interop;

namespace Stacky.Compilation.LLVM;

public record NativeFunction(IReadOnlyList<LLVMTypeRef> Inputs, LLVMTypeRef? Output, bool HasVarArgs) : CompilerType;