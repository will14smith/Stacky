namespace Stacky.Language.Compilation;

public abstract record CompilerType
{
    public record String : CompilerType;

    public record Long : CompilerType;

    public record Function(IReadOnlyList<CompilerType> Inputs, IReadOnlyList<CompilerType> Outputs) : CompilerType;
}