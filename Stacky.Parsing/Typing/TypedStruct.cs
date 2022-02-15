using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public record TypedStruct(SyntaxStruct Syntax, StackyType.Struct Type, IReadOnlyList<TypedStructField> Fields)
{
    public SyntaxExpression.Identifier Name => Syntax.Name;
}

public record TypedStructField(SyntaxStructField Syntax, StackyType Type)
{
    public SyntaxExpression.Identifier Name => Syntax.Name;
}
