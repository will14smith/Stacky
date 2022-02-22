namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    public CompilerValue LoadIndex(CompilerValue target, CompilerValue index)
    {
        var ptr = _builder.CreateGEP(target.Value, new[] { index.Value }, "offsetString");
        
        var value = _builder.CreateLoad(ptr);
        
        // TODO assuming strings for now, expand to more array types
        return new CompilerValue(value, new CompilerType.Byte());
    }
    
    public void StoreIndex(CompilerValue target, CompilerValue index, CompilerValue value)
    {
        var ptr = _builder.CreateGEP(target.Value, new[] { index.Value }, "offsetString");
        
        _builder.CreateStore(value.Value, ptr);
    }
    
    public void Copy(CompilerValue target, CompilerValue targetOffset, CompilerValue source, CompilerValue sourceOffset, CompilerValue length)
    {
        var targetPtr = _builder.CreateGEP(target.Value, new[] { targetOffset.Value }, "offsetTarget");
        var sourcePtr = _builder.CreateGEP(source.Value, new[] { sourceOffset.Value }, "offsetSource");

        _builder.CreateMemCpy(targetPtr, 0, sourcePtr, 0, length.Value);
    }
}