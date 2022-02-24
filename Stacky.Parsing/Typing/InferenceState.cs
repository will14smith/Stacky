using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public class InferenceState
{
    private readonly ImmutableList<StackyType.Variable> _variables;
    private readonly ImmutableList<InferenceConstraint> _constraints;
    private readonly ImmutableStack<IReadOnlyDictionary<string, StackyType>> _bindings;

    public SyntaxProgram Program { get; }
    public IReadOnlyList<StackyType.Variable> Variables => _variables.ToList();
    public IReadOnlyList<InferenceConstraint> Constraints => _constraints.ToList();

    public InferenceState(SyntaxProgram program) : this(program, ImmutableList<StackyType.Variable>.Empty, ImmutableList<InferenceConstraint>.Empty, ImmutableStack<IReadOnlyDictionary<string, StackyType>>.Empty) { }
    private InferenceState(SyntaxProgram program, ImmutableList<StackyType.Variable> variables, ImmutableList<InferenceConstraint> constraints, ImmutableStack<IReadOnlyDictionary<string, StackyType>> bindings)
    {
        Program = program;
        _variables = variables;
        _constraints = constraints;
        _bindings = bindings;
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
    public SyntaxStruct LookupStruct(string structName)
    {
        var structDef = Program.Structs.FirstOrDefault(x => x.Name.Value == structName);
        
        if (structDef == null)
        {
            throw new TypeInferenceException($"Failed to find struct '{structName}' to initialise");
        }
        
        return structDef;
    }

    [Pure]
    public bool TryLookupBinding(string name, out StackyType? type)
    {
        foreach (var bindings in _bindings)
        {
            if (bindings.TryGetValue(name, out type))
            {
                return true;
            }
        }

        type = default;
        return false;
    }
    
    [Pure]
    public IReadOnlyList<StackyBinding> GetBindings()
    {
        var seen = new HashSet<string>();
        var bindings = new List<StackyBinding>();
        
        foreach (var scope in _bindings)
        {
            foreach (var (name, type) in scope)
            {
                if (seen.Add(name))
                {
                    bindings.Add(new StackyBinding(name, type));
                }
            }
        }
        
        return bindings;
    }

    [Pure]
    public InferenceState PushBindings(IReadOnlyDictionary<string, StackyType> newBindings) => WithBindings(_bindings.Push(newBindings));

    [Pure]
    public InferenceState PopBindings() => WithBindings(_bindings.Pop());

    [Pure]
    private InferenceState WithVariables(ImmutableList<StackyType.Variable> variables) => new(Program, variables, _constraints, _bindings);
    [Pure]
    private InferenceState WithConstraints(ImmutableList<InferenceConstraint> constraints) => new(Program, _variables, constraints, _bindings);
    [Pure]
    private InferenceState WithBindings(ImmutableStack<IReadOnlyDictionary<string, StackyType>> bindings) => new(Program, _variables, _constraints, bindings);
}