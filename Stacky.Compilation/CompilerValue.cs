using LLVMSharp;

namespace Stacky.Compilation;

public struct CompilerValue
{
    internal CompilerValue(Value value, CompilerType type)
    {
        Value = value;
        Type = type;
    }

    internal Value Value { get; }
    public CompilerType Type { get; }
}