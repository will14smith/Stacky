using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public record TypedFunction(SyntaxFunction Syntax, StackyType.Function Type, TypedExpression Body)
{
    
}