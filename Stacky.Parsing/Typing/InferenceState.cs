using System.Collections.Immutable;
using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public class InferenceState
{
    private readonly ImmutableList<StackyType.Variable> _variables;
    private readonly ImmutableList<InferenceConstraint> _constraints;

    public SyntaxProgram Program { get; }
    public IReadOnlyList<InferenceConstraint> Constraints => _constraints.ToList();

    public InferenceState(SyntaxProgram program) : this(program, ImmutableList<StackyType.Variable>.Empty, ImmutableList<InferenceConstraint>.Empty) { }
    private InferenceState(SyntaxProgram program, ImmutableList<StackyType.Variable> variables, ImmutableList<InferenceConstraint> constraints)
    {
        Program = program;
        _variables = variables;
        _constraints = constraints;
    }

    public InferenceState NewVariable(StackySort sort, out StackyType type)
    {
        var variable = new StackyType.Variable(_variables.Count, sort);
        type = variable;
        return WithVariables(_variables.Add(variable));
    }
    
    public InferenceState Unify(StackyType left, StackyType right)
    {
        var constraint = new InferenceConstraint(left, right);
        return WithConstraints(_constraints.Add(constraint));
    }
    
    private InferenceState WithVariables(ImmutableList<StackyType.Variable> variables) => new(Program, variables, _constraints);
    private InferenceState WithConstraints(ImmutableList<InferenceConstraint> constraints) => new(Program, _variables, constraints);
}