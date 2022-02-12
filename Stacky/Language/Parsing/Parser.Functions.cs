using Stacky.Language.Syntax;

namespace Stacky.Language.Parsing;

public partial class Parser
{
    private SyntaxFunction ParseFunction(ref State state)
    {
        var start = state.Location;

        var name = ParseIdentifier(ref state);
        var type = ParseTypeFunction(ref state);
        
        Consume(ref state, '{');

        var body = ParseExpression(ref state);

        Consume(ref state, '}');

        var position = state.PositionFromStart(start);
        return new SyntaxFunction(position, name, type, body);
    }
}