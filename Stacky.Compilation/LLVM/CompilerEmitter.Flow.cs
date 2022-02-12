using L = LLVMSharp.LLVM;

namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    public void Branch(CompilerValue condition, CompilerLabel trueBlock, CompilerLabel falseBlock)
    {
        L.BuildCondBr(_builder, condition.Value, trueBlock.Block, falseBlock.Block);
    }

    public void Branch(CompilerLabel targetBlock)
    {
        L.BuildBr(_builder, targetBlock.Block);
    }
}