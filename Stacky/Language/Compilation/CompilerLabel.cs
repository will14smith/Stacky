using LLVMSharp;

namespace Stacky.Language.Compilation;

public struct CompilerLabel
{
    internal CompilerLabel(LLVMBasicBlockRef block)
    {
        Block = block;
    }

    internal LLVMBasicBlockRef Block { get; }
}