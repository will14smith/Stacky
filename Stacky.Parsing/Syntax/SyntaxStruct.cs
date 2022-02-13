namespace Stacky.Parsing.Syntax;

public record SyntaxStruct(SyntaxPosition Position, SyntaxExpression.Identifier Name, IReadOnlyList<SyntaxStructField> Fields) : SyntaxElement(Position);
public record SyntaxStructField(SyntaxPosition Position, SyntaxExpression.Identifier Name, SyntaxType Type) : SyntaxElement(Position);
