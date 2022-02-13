namespace Stacky.Parsing.Typing;

public class InferenceSolver
{
    public static InferenceSubstitutions Solve(InferenceState state)
    {
        var substitutions = new Dictionary<StackyType.Variable, StackyType>();
        var constraints = new Stack<InferenceConstraint>(state.Constraints);

        while (constraints.Count > 0)
        {
            var constraint = Apply(constraints.Pop(), substitutions);

            var (newSubstitutions, newConstraints) = Solve(constraint);
            
            foreach (var (variable, previous) in substitutions)
            {
                substitutions[variable] = InferenceSubstitutions.Apply(newSubstitutions, previous);
            }
            
            foreach (var (variable, substitution) in newSubstitutions)
            {
                substitutions[variable] = substitution;
            }
            
            for (var i = newConstraints.Count - 1; i >= 0; i--)
            {
                constraints.Push(newConstraints[i]);
            }
        }
        
        return new InferenceSubstitutions(substitutions);
    }

    private static InferenceConstraint Apply(InferenceConstraint constraint, IReadOnlyDictionary<StackyType.Variable, StackyType> substitutions)
    {
        if (!substitutions.Any())
        {
            return constraint;
        }

        return new InferenceConstraint(
            InferenceSubstitutions.Apply(substitutions, constraint.Left), 
            InferenceSubstitutions.Apply(substitutions, constraint.Right));
    }

    private static (IReadOnlyDictionary<StackyType.Variable, StackyType> Substitutions, IReadOnlyList<InferenceConstraint> Constraints) Solve(InferenceConstraint constraint)
    {
        if (constraint.Left == constraint.Right)
        {
            return (new Dictionary<StackyType.Variable, StackyType>(), Array.Empty<InferenceConstraint>());
        }

        if (constraint.Left is StackyType.Variable leftVar)
        {
            return Replace(leftVar, constraint.Right);
        }
        if (constraint.Right is StackyType.Variable rightVar)
        {
            return Replace(rightVar, constraint.Left);
        }

        if (constraint.Left is StackyType.Composite leftComp && constraint.Right is StackyType.Composite rightComp)
        {
            return SolveComposite(leftComp, rightComp);
        }
        
        if (constraint.Left is StackyType.Function leftFunc && constraint.Right is StackyType.Function rightFunc)
        {
            return SolveFunction(leftFunc, rightFunc);
        }
        
        throw new NotImplementedException("handle invalid cases (e.g. str == i64)");
    }
    
    private static (IReadOnlyDictionary<StackyType.Variable, StackyType> Substitutions, IReadOnlyList<InferenceConstraint> Constraints) Replace(StackyType.Variable variable, StackyType type)
    {
        if (variable.Sort is not null)
        {
            // TODO check compatible
        }
        
        return (new Dictionary<StackyType.Variable, StackyType> { { variable, type } }, Array.Empty<InferenceConstraint>());
    }
    
    private static (IReadOnlyDictionary<StackyType.Variable, StackyType> Substitutions, IReadOnlyList<InferenceConstraint> Constraints) SolveComposite(StackyType.Composite left, StackyType.Composite right)
    {
        var leftTypes = left.Types;
        var rightTypes = right.Types;

        if (leftTypes.Count != rightTypes.Count)
        {
            throw new NotSupportedException("this could be possible if a variable in the composite is a composite itself");
        }

        var constraints = leftTypes.Zip(rightTypes).Select(x => new InferenceConstraint(x.First, x.Second)).ToList();
        
        return (new Dictionary<StackyType.Variable, StackyType>(), constraints);
    }
    
    private static (IReadOnlyDictionary<StackyType.Variable, StackyType> Substitutions, IReadOnlyList<InferenceConstraint> Constraints) SolveFunction(StackyType.Function left, StackyType.Function right)
    {
        var constraints = new[]
        {
            new InferenceConstraint(left.Input, right.Input),
            new InferenceConstraint(left.Output, right.Output),
        };
        
        return (new Dictionary<StackyType.Variable, StackyType>(), constraints);
    }

}