using LLVMSharp;

namespace Stacky.Compilation;

public struct CompilerLabel
{
    internal CompilerLabel(BasicBlock block)
    {
        Block = block;
    }

    internal BasicBlock Block { get; }
}