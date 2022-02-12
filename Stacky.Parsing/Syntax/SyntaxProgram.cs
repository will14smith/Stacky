namespace Stacky.Parsing.Syntax;

public record SyntaxProgram(SyntaxPosition Position, IReadOnlyList<SyntaxFunction> Functions) : SyntaxElement(Position);