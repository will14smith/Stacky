using Stacky.Parsing.Syntax;

namespace Stacky.Parsing;

public partial class Parser
{
    private static SyntaxExpression ParseExpression(ref State state)
    {
        var atoms = new List<SyntaxExpression>();
     
        var start = state.Location;
        while (TryParseExpressionAtom(ref state, out var atom))
        {
            atoms.Add(atom!);
        }

        if (atoms.Count == 1)
        {
            return atoms[0];
        }
        
        var position = state.PositionFromStart(start);
        return new SyntaxExpression.Application(position, atoms);
    }

    private static bool TryParseExpressionAtom(ref State state, out SyntaxExpression? expression)
    {
        state = state.SkipWhiteSpace();

        if (state.IsEof)
        {
            expression = null;
            return false;
        }
        
        var current = state.Current;

        if (char.IsDigit(current))
        {
            expression = ParseNumber(ref state);
            return true;
        }

        if (current == '"')
        {
            expression = ParseString(ref state);
            return true;
        }
        
        if (current == '{')
        {
            expression = ParseAnonymousFunction(ref state);
            return true;
        }

        if (current == '(')
        {
            expression = ParseBinding(ref state);
            return true;
        }

        if (IsIdentifierStart(current))
        {
            expression = ParseIdentifier(ref state);
            return true;
        }

        expression = default;
        return false;
    }

    private static SyntaxExpression ParseNumber(ref State state)
    {
        state = state.SkipWhiteSpace();
        var start = state.Location;
        
        while (TryConsume(ref state, char.IsDigit, skipWhiteSpace: false)) { }

        var position = state.PositionFromStart(start);
        var text = state.GetText(position);
        if (!long.TryParse(text, out var value))
        {
            throw Error(state, position, $"Invalid number format: '{text}'");
        }
        
        return new SyntaxExpression.LiteralInteger(position, value);

    }

    private static SyntaxExpression ParseString(ref State state)
    {
        state = state.SkipWhiteSpace();
        var start = state.Location;

        Consume(ref state, '"');
        while (TryConsume(ref state, c => c != '"', skipWhiteSpace: false)) { }
        Consume(ref state, '"');

        var position = state.PositionFromStart(start);
        // TODO handle escapes
        var text = state.GetText(position)[1..^1];
        return new SyntaxExpression.LiteralString(position, text);
    }

    private static SyntaxExpression.Identifier ParseIdentifier(ref State state)
    {
        state = state.SkipWhiteSpace();
        var start = state.Location;

        Consume(ref state, IsIdentifierStart, "Expected Identifier Start");
        while (TryConsume(ref state, IsIdentifier, skipWhiteSpace: false)) { }

        var position = state.PositionFromStart(start);
        var name = state.GetText(position);
        return new SyntaxExpression.Identifier(position, name);
    }

    private static bool IsIdentifierStart(char c) => c is not ('"' or '{' or '}' or '(' or ')') && !char.IsDigit(c) && !char.IsWhiteSpace(c);
    private static bool IsIdentifier(char c) => c is not ('}' or ')') && !char.IsWhiteSpace(c);
}