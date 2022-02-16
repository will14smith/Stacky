using LLVMSharp;

namespace Stacky.Compilation;

public abstract record CompilerType
{
    public record String : CompilerType;

    public record Boolean : CompilerType;
    public record Long : CompilerType;

    public record Function(IReadOnlyList<CompilerType> Inputs, IReadOnlyList<CompilerType> Outputs) : CompilerType;

    public sealed record Struct(string Name, IReadOnlyList<StructField> Fields) : CompilerType
    {
        public bool Equals(Struct? other) => other != null && other.Name == Name;
        public override int GetHashCode() => Name.GetHashCode();
    }
    
    
    public record StructField(string Name, CompilerType Type);

    public record Pointer(CompilerType Type) : CompilerType;
}