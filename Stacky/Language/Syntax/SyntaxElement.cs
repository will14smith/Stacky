namespace Stacky.Language.Syntax;

public record SyntaxLocation(string? File, int Offset, int Line, int LineOffset)
{
    public static readonly SyntaxLocation Empty = new SyntaxLocation("<unknown>", 0, 0, 0);
}

public record SyntaxPosition(SyntaxLocation Start, SyntaxLocation End)
{
    public static readonly SyntaxPosition Empty = new SyntaxPosition(SyntaxLocation.Empty, SyntaxLocation.Empty);
}

public abstract record SyntaxElement(SyntaxPosition Position);