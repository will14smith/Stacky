namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    public void Branch(CompilerValue condition, CompilerLabel trueBlock, CompilerLabel falseBlock) => _builder.CreateCondBr(condition.Value, trueBlock.Block, falseBlock.Block);
    public void Branch(CompilerLabel targetBlock) => _builder.CreateBr(targetBlock.Block);
}