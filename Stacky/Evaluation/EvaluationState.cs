using System.Collections.Immutable;
using Stacky.Parsing.Syntax;

namespace Stacky.Evaluation;

public class EvaluationState
{
    private readonly SyntaxProgram _program;
    private readonly ImmutableStack<EvaluationValue> _stack;

    public EvaluationState(SyntaxProgram program) : this(program, ImmutableStack<EvaluationValue>.Empty) { }

    private EvaluationState(SyntaxProgram program, ImmutableStack<EvaluationValue> stack)
    {
        _program = program;
        _stack = stack;
    }

    public SyntaxFunction GetFunction(string name)
    {
        return _program.Functions.FirstOrDefault(x => x.Name.Value == name) ?? throw new Exception($"Function '{name}' was not declared");
    }

    public EvaluationState Push(EvaluationValue value)
    {
        return new EvaluationState(_program, _stack.Push(value));
    }      
    public EvaluationState Pop(out EvaluationValue value)
    {
        var stack = _stack.Pop(out value);

        return new EvaluationState(_program, stack);
    }
}