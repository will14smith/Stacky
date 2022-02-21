using Stacky.Parsing.Syntax;

namespace Stacky.Parsing;

public partial class Parser
{
    private static SyntaxExpression ParseBinding(ref State state)
    {      
        var start = state.Location;

        Consume(ref state, '(');
        var names = ParseBindingNames(ref state);
        Consume(ref state, ')');

        Consume(ref state, '{');
        var body = ParseExpression(ref state);
        Consume(ref state, '}');
        
        var position = state.PositionFromStart(start);
        return new SyntaxExpression.Binding(position, names, body);
    }

    private static IReadOnlyList<SyntaxExpression.Identifier> ParseBindingNames(ref State state)
    {
        var names = new List<SyntaxExpression.Identifier>();
        
        while (!Peek(ref state, c => c == ')'))
        {
            names.Add(ParseIdentifier(ref state));
        }

        return names;
    }
}