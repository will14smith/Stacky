using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public abstract record TypedExpression(SyntaxExpression Syntax, StackyType Type)
{
    public record LiteralInteger(SyntaxExpression Syntax, StackyType Type) : TypedExpression(Syntax, Type);
    public record LiteralString(SyntaxExpression Syntax, StackyType Type) : TypedExpression(Syntax, Type);
    public record Identifier(SyntaxExpression Syntax, StackyType Type) : TypedExpression(Syntax, Type);
    public record Application(SyntaxExpression Syntax, StackyType Type, IReadOnlyList<TypedExpression> Expressions) : TypedExpression(Syntax, Type);
    public record Function(SyntaxExpression Syntax, StackyType Type) : TypedExpression(Syntax, Type);

}