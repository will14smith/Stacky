namespace Stacky.Compilation;

public abstract record CompilerType
{
    public record String : CompilerType;

    public record Boolean : CompilerType;
    public record Long : CompilerType;

    public record Function(IReadOnlyList<CompilerType> Inputs, IReadOnlyList<CompilerType> Outputs) : CompilerType;
}