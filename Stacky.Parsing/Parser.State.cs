using Stacky.Parsing.Syntax;

namespace Stacky.Parsing;

public partial class Parser
{
    private State NewState() => new State(_name, _code);

    private class State
    {
        private int _offset;
        private (int Line, int Offset) _line;

        private readonly string _name;
        private readonly string _code;

        public State(string name, string code)
        {
            _offset = 0;
            _line = (1, 0);

            _name = name;
            _code = code;
        }

        public SyntaxLocation Location => new(_name, _offset, _line.Line, _line.Offset);

        public char Current => IsEof ? '\0' : _code[_offset];
        public bool IsEof => _offset >= _code.Length;

        public SyntaxPosition PositionFromStart(SyntaxLocation start) => new(start, Location);
        public string GetText(SyntaxPosition position) => _code[position.Start.Offset..position.End.Offset];

        public State Advance()
        {
            var line = Current == '\n' ? (_line.Line + 1, 0) : (_line.Line, _line.Offset + 1);
            
            var next = new State(_name, _code)
            {
                _offset = _offset + 1,
                _line = line,
            };
            
            return next;
        }

        public State SkipWhiteSpace()
        {
            var state = this;

            while (char.IsWhiteSpace(state.Current))
            {
                state = state.Advance();
            }

            return state;
        }
    }
}