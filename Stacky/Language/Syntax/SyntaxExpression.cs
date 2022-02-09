namespace Stacky.Language.Syntax;

public abstract record SyntaxExpression(SyntaxPosition Position) : SyntaxElement(Position)
{
    public record LiteralInteger(SyntaxPosition Position, long Value) : SyntaxExpression(Position);
    public record LiteralString(SyntaxPosition Position, string Value) : SyntaxExpression(Position);
    public record Identifier(SyntaxPosition Position, string Value) : SyntaxExpression(Position);
    public record Application(SyntaxPosition Position, IReadOnlyList<SyntaxExpression> Expressions) : SyntaxExpression(Position);
    public record Conditional(SyntaxPosition Position, SyntaxExpression Condition, SyntaxExpression True, SyntaxExpression? False) : SyntaxExpression(Position);
}