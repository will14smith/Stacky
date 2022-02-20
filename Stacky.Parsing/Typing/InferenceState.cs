using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public class InferenceState
{
    private readonly ImmutableList<StackyType.Variable> _variables;
    private readonly ImmutableList<InferenceConstraint> _constraints;

    public SyntaxProgram Program { get; }
    public IReadOnlyList<StackyType.Variable> Variables => _variables.ToList();
    public IReadOnlyList<InferenceConstraint> Constraints => _constraints.ToList();

    public InferenceState(SyntaxProgram program) : this(program, ImmutableList<StackyType.Variable>.Empty, ImmutableList<InferenceConstraint>.Empty) { }
    private InferenceState(SyntaxProgram program, ImmutableList<StackyType.Variable> variables, ImmutableList<InferenceConstraint> constraints)
    {
        Program = program;
        _variables = variables;
        _constraints = constraints;
    }

    [Pure]
    public InferenceState NewStackVariable(out StackyType type) => NewVariable(new StackySort.Stack(), out type);

    [Pure]
    public InferenceState NewVariable(StackySort sort, out StackyType type)
    {
        var variable = new StackyType.Variable(_variables.Count, sort);
        type = variable;
        return WithVariables(_variables.Add(variable));
    }
    
    [Pure]
    public InferenceState Unify(StackyType left, StackyType right)
    {
        var constraint = new InferenceConstraint(left, right);
        return WithConstraints(_constraints.Add(constraint));
    }
    
    [Pure]
    private InferenceState WithVariables(ImmutableList<StackyType.Variable> variables) => new(Program, variables, _constraints);
    [Pure]
    private InferenceState WithConstraints(ImmutableList<InferenceConstraint> constraints) => new(Program, _variables, constraints);

    [Pure]
    public SyntaxStruct LookupStruct(string structName)
    {
        var structDef = Program.Structs.FirstOrDefault(x => x.Name.Value == structName);
        
        if (structDef == null)
        {
            throw new TypeInferenceException($"Failed to find struct '{structName}' to initialise");
        }
        
        return structDef;
    }
}