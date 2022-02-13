using Stacky.Parsing.Syntax;

namespace Stacky.Parsing;

public partial class Parser
{
    private SyntaxStruct ParseStruct(ref State state, SyntaxElement identifier)
    {
        var start = identifier.Position.Start;

        var name = ParseIdentifier(ref state);
        
        Consume(ref state, '{');

        var fields = ParseStructFields(ref state);

        Consume(ref state, '}');

        var position = state.PositionFromStart(start);
        return new SyntaxStruct(position, name, fields);

    }

    private IReadOnlyList<SyntaxStructField> ParseStructFields(ref State state)
    {
        var fields = new List<SyntaxStructField>();
        
        while (!Peek(ref state, c => c == '}'))
        {
            fields.Add(ParseStructField(ref state));
        }

        return fields;
    }

    private SyntaxStructField ParseStructField(ref State state)
    {
        var start = state.Location;
        
        var name = ParseIdentifier(ref state);
        var type = ParseType(ref state);

        var position = state.PositionFromStart(start);
        return new SyntaxStructField(position, name, type);
    }
}