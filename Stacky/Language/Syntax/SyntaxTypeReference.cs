namespace Stacky.Language.Syntax;

public record SyntaxTypeReference(SyntaxPosition Position, SyntaxType Type) : SyntaxElement(Position);

public record SyntaxType
{
    public record Integer(bool Signed, IntegerSize Size) : SyntaxType;
    public record String : SyntaxType;

    public enum IntegerSize
    {
        S8,
        S16,
        S32,
        S64,
    }
}
