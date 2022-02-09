namespace Stacky.Language.Syntax;

public record SyntaxFunction(SyntaxPosition Position, SyntaxExpression.Identifier Name, IReadOnlyCollection<SyntaxTypeReference> Input, IReadOnlyCollection<SyntaxTypeReference> Output, SyntaxExpression Body) : SyntaxElement(Position);