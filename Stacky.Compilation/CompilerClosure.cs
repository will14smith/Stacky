using Stacky.Compilation.LLVM;

namespace Stacky.Compilation;

public record CompilerClosure(CompilerStruct Closure, CompilerStruct State, CompilerValue Function);