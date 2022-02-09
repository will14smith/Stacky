using Stacky.Language.Syntax;

namespace Stacky.Language.Parsing;

public partial class Parser
{
    private SyntaxFunction ParseFunction(ref State state)
    {
        var start = state.Location;

        var input = ParseFunctionTypes(ref state);
        var name = ParseIdentifier(ref state);
        var output = ParseFunctionTypes(ref state);

        Consume(ref state, '{');

        var body = ParseExpression(ref state);

        Consume(ref state, '}');

        var position = state.PositionFromStart(start);
        return new SyntaxFunction(position, name, input, output, body);
    }

    private IReadOnlyCollection<SyntaxTypeReference> ParseFunctionTypes(ref State state)
    {
        if (!TryConsume(ref state, '('))
        {
            return new[] { ParseFunctionType(ref state) };
        }

        var types = new List<SyntaxTypeReference>();
        
        while (!TryConsume(ref state, ')'))
        {
            types.Add(ParseFunctionType(ref state));
        }

        return types;
    }

    private SyntaxTypeReference ParseFunctionType(ref State state)
    {
        var ident = ParseIdentifier(ref state);

        var type = ident.Value switch
        {
            "i64" => new SyntaxType.Integer(true, SyntaxType.IntegerSize.S64),
            
            _ => throw Error(state, ident.Position, $"Unknown type '{ident.Value}'"),
        };

        return new SyntaxTypeReference(ident.Position, type);
    }
}