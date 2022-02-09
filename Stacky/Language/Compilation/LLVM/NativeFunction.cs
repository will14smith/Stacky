using LLVMSharp;

namespace Stacky.Language.Compilation.LLVM;

public record NativeFunction(IReadOnlyList<LLVMTypeRef> Inputs, LLVMTypeRef? Output, bool HasVarArgs) : CompilerType;