namespace Stacky.Language.Syntax;

public abstract record SyntaxType(SyntaxPosition Position) : SyntaxElement(Position)
{
    public record Boolean(SyntaxPosition Position) : SyntaxType(Position);
    public record Integer(SyntaxPosition Position, bool Signed, IntegerSize Size) : SyntaxType(Position);
    public record String(SyntaxPosition Position) : SyntaxType(Position);
    public record Function(SyntaxPosition Position, IReadOnlyList<SyntaxType> Input, IReadOnlyList<SyntaxType> Output) : SyntaxType(Position);
    
    public enum IntegerSize
    {
        S8,
        S16,
        S32,
        S64,
    }
}
