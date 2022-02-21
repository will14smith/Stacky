using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public abstract record TypedExpression(SyntaxExpression Syntax, StackyType Type)
{
    public record LiteralInteger(SyntaxExpression Syntax, StackyType Type) : TypedExpression(Syntax, Type)
    {
        public long Value => ((SyntaxExpression.LiteralInteger)Syntax).Value;
    }

    public record LiteralString(SyntaxExpression Syntax, StackyType Type) : TypedExpression(Syntax, Type)
    {
        public string Value => ((SyntaxExpression.LiteralString)Syntax).Value;
    }

    public record Identifier(SyntaxExpression Syntax, StackyType Type) : TypedExpression(Syntax, Type)
    {
        public string Value => ((SyntaxExpression.Identifier)Syntax).Value;
    }
    
    public record Application(SyntaxExpression Syntax, StackyType Type, IReadOnlyList<TypedExpression> Expressions) : TypedExpression(Syntax, Type);
    public record Function(SyntaxExpression Syntax, StackyType Type, TypedExpression Body) : TypedExpression(Syntax, Type);
    public record Binding(SyntaxExpression Syntax, StackyType Type, IReadOnlyList<Identifier> Names, TypedExpression Body) : TypedExpression(Syntax, Type);

}