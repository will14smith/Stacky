using Stacky.Parsing.Syntax;

namespace Stacky.Parsing;

public partial class Parser
{
    private static SyntaxFunction ParseFunction(ref State state, SyntaxExpression.Identifier name)
    {
        var start = name.Position.Start;

        var type = ParseTypeFunction(ref state);
        
        Consume(ref state, '{');

        var body = ParseExpression(ref state);

        Consume(ref state, '}');

        var position = state.PositionFromStart(start);
        return new SyntaxFunction(position, name, type, body);
    }

    private static SyntaxExpression ParseAnonymousFunction(ref State state)
    {
        var start = state.Location;

        Consume(ref state, '{');

        var body = ParseExpression(ref state);
        
        Consume(ref state, '}');

        var position = state.PositionFromStart(start);
        return new SyntaxExpression.Function(position, body);
    }
}