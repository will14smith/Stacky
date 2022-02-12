using Stacky.Language.Syntax;

namespace Stacky.Language.Parsing;

public partial class Parser
{
    private readonly string _name;
    private readonly string _code;

    public Parser(string name, string code)
    {
        _name = name;
        _code = code;
    }

    public SyntaxProgram Parse() => ParseProgram(NewState());
    

    private SyntaxProgram ParseProgram(State state)
    {
        var start = state.Location;
        var functions = new List<SyntaxFunction>();

        while (!state.IsEof)
        {
            var function = ParseFunction(ref state);
            functions.Add(function);

            state = state.SkipWhiteSpace();
        }
        
        var position = state.PositionFromStart(start);
        return new SyntaxProgram(position, functions);
    }

    private static bool Peek(ref State state, Func<object, bool> predicate, bool skipWhiteSpace = true)
    {
        if (state.IsEof)
        {
            return false;
        }

        if (skipWhiteSpace)
        {
            state = state.SkipWhiteSpace();
        }

        return predicate(state.Current);
    }
    
    private static void Consume(ref State state, char expected, bool skipWhiteSpace = true) => Consume(ref state, c => c == expected, $"Expected '{expected}'", skipWhiteSpace);
    private static void Consume(ref State state, Func<char, bool> predicate, string message, bool skipWhiteSpace = true)
    {
        if (!TryConsume(ref state, predicate, skipWhiteSpace))
        {
            throw Error(state, message);
        }
    }
    
    private static bool TryConsume(ref State state, char expected, bool skipWhiteSpace = true) => TryConsume(ref state, c => c == expected, skipWhiteSpace);
    private static bool TryConsume(ref State state, Func<char, bool> predicate, bool skipWhiteSpace = true)
    {
        if (state.IsEof)
        {
            return false;
        }

        if (skipWhiteSpace)
        {
            state = state.SkipWhiteSpace();
        }

        if(!predicate(state.Current))
        {
            return false;
        }
        
        state = state.Advance();

        return true;
    }
    
    private static Exception Error(State state, string message)
    {
        return new Exception($"{message} at {state.Location}");
    }
    private static Exception Error(State state, SyntaxLocation location, string message)
    {
        return new Exception($"{message} at {location}");
    }
    private static Exception Error(State state, SyntaxPosition position, string message)
    {
        return new Exception($"{message} at {position}");
    }
}