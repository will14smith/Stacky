using LLVMSharp;

namespace Stacky.Compilation;

public abstract record CompilerType
{
    public record String : CompilerType
    {
        public override string ToString() => "str";
    }

    public record Boolean : CompilerType
    {
        public override string ToString() => "bool";
    }

    public record Byte : CompilerType, IComparableType, INumericType
    {
        public override string ToString() => "u8";
    }

    public record Int : CompilerType, IComparableType, INumericType
    {
        public override string ToString() => "i32";
    }

    public record Long : CompilerType, IComparableType, INumericType
    {
        public override string ToString() => "i64";
    }

    public record Function(IReadOnlyList<CompilerType> Inputs, IReadOnlyList<CompilerType> Outputs) : CompilerType;

    public sealed record Struct(string Name, IReadOnlyList<StructField> Fields) : CompilerType
    {
        public bool Equals(Struct? other) => other != null && other.Name == Name;
        public override int GetHashCode() => Name.GetHashCode();
    }
    
    public record StructField(string Name, CompilerType Type);

    public record Pointer(CompilerType Type) : CompilerType;
}

public interface IComparableType {}
public interface INumericType {}