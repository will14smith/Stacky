using Stacky.Parsing.Syntax;

namespace Stacky.Parsing;

public partial class Parser
{
    private static SyntaxType ParseType(ref State state)
    {
        if (TryConsume(ref state, '('))
        {
            var function = ParseTypeFunction(ref state);
            
            Consume(ref state, ')');

            return function;
        }

        var identifier = ParseIdentifier(ref state);

        return identifier.Value switch
        {
            "bool" => new SyntaxType.Boolean(identifier.Position), 
            "str" => new SyntaxType.String(identifier.Position),

            "i8" => new SyntaxType.Integer(identifier.Position, true, SyntaxType.IntegerSize.S8), 
            "u8" => new SyntaxType.Integer(identifier.Position, false, SyntaxType.IntegerSize.S8), 
            "i16" => new SyntaxType.Integer(identifier.Position, true, SyntaxType.IntegerSize.S16), 
            "u16" => new SyntaxType.Integer(identifier.Position, false, SyntaxType.IntegerSize.S16), 
            "i32" => new SyntaxType.Integer(identifier.Position, true, SyntaxType.IntegerSize.S32), 
            "u32" => new SyntaxType.Integer(identifier.Position, false, SyntaxType.IntegerSize.S32), 
            "i64" => new SyntaxType.Integer(identifier.Position, true, SyntaxType.IntegerSize.S64), 
            "u64" => new SyntaxType.Integer(identifier.Position, false, SyntaxType.IntegerSize.S64),

            _ => new SyntaxType.Struct(identifier.Position, identifier.Value),
        };
    }

    private static SyntaxType.Function ParseTypeFunction(ref State state)
    {
        var start = state.Location;
        var input = ParseTypeList(ref state);

        Consume(ref state, '-');
        Consume(ref state, '>', false);

        var output = ParseTypeList(ref state);

        var position = state.PositionFromStart(start);
        return new SyntaxType.Function(position, input, output);
    }

    private static IReadOnlyList<SyntaxType> ParseTypeList(ref State state)
    {
        var types = new List<SyntaxType>();
        
        if (TryConsume(ref state, '('))
        {
            if (TryConsume(ref state, ')'))
            {
                return types;
            }

            // we've hit a function so continue parsing it before doing the main loop
            
            var function = ParseTypeFunction(ref state);
            types.Add(function);
            
            Consume(ref state, ')');
        }
        
        // TODO this end condition is a bit nasty...
        while (!Peek(ref state, x => x is '-' or ')' or '{'))
        {
            var type = ParseType(ref state);
            types.Add(type);
        }

        if (types.Count == 0)
        {
            // this should have been a `()`
            throw Error(state, "Expected a type or an explicit void '()'");
        }

        return types;
    }
}