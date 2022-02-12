namespace Stacky.Language.Syntax;

public record SyntaxFunction(SyntaxPosition Position, SyntaxExpression.Identifier Name, SyntaxType.Function Type, SyntaxExpression Body) : SyntaxElement(Position)
{
    public IReadOnlyList<SyntaxType> Input => Type.Input;
    public IReadOnlyList<SyntaxType> Output => Type.Output;
}