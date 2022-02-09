namespace Stacky.Language.Syntax;

public record SyntaxLocation(string? File, int Offset, int Line, int LineOffset);
public record SyntaxPosition(SyntaxLocation Start, SyntaxLocation End);
public abstract record SyntaxElement(SyntaxPosition Position);