using System.Collections.Immutable;
using Stacky.Parsing.Syntax;

namespace Stacky.Evaluation;

public class EvaluationState
{
    private readonly SyntaxProgram _program;
    public ImmutableStack<EvaluationValue> Stack { get; }

    public EvaluationState(SyntaxProgram program) : this(program, ImmutableStack<EvaluationValue>.Empty) { }

    public EvaluationState(SyntaxProgram program, ImmutableStack<EvaluationValue> stack)
    {
        _program = program;
        Stack = stack;
    }


    public SyntaxFunction GetFunction(string name)
    {
        return _program.Functions.FirstOrDefault(x => x.Name.Value == name) ?? throw new Exception($"Function '{name}' was not declared");
    }

    public EvaluationState Push(EvaluationValue value)
    {
        return new EvaluationState(_program, Stack.Push(value));
    }      
    public EvaluationState Pop(out EvaluationValue value)
    {
        var stack = Stack.Pop(out value);

        return new EvaluationState(_program, stack);
    }
}