using LLVMSharp;

namespace Stacky.Language.Compilation;

public struct CompilerValue
{
    internal CompilerValue(LLVMValueRef value, CompilerType type)
    {
        Value = value;
        Type = type;
    }

    internal LLVMValueRef Value { get; }
    public CompilerType Type { get; }
}