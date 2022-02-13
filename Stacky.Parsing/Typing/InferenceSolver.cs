using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public class InferenceSolver
{
    public static InferenceSubstitutions Solve(InferenceState state)
    {
        var substitutions = new Dictionary<int, StackyType>();
        var constraints = new Stack<InferenceConstraint>(state.Constraints);

        // resolve all constraints
        while (constraints.Count > 0)
        {
            var constraint = Apply(constraints.Pop(), substitutions);

            var (newSubstitutions, newConstraints) = Solve(constraint);
            
            UpdateSubstitutions(substitutions, newSubstitutions);
            
            foreach (var (variable, substitution) in newSubstitutions)
            {
                substitutions[variable] = substitution;
            }
            
            for (var i = newConstraints.Count - 1; i >= 0; i--)
            {
                constraints.Push(newConstraints[i]);
            }
        }

        // ensure all variables got a resolved type
        var variables = state.Variables;
        foreach (var variable in variables)
        {
            var substitutedVariable = InferenceSubstitutions.Apply(substitutions, variable);

            if (substitutedVariable is StackyType.Variable remainingVariable)
            {
                UpdateSubstitutions(substitutions, new Dictionary<int, StackyType>
                {
                    { variable.Id, FindTypeUsingSort(remainingVariable) }
                });
            }
        }
        
        return new InferenceSubstitutions(substitutions);
    }

    private static void UpdateSubstitutions(Dictionary<int, StackyType> substitutions, IReadOnlyDictionary<int, StackyType> newSubstitutions)
    {
        foreach (var (variable, previous) in substitutions)
        {
            substitutions[variable] = InferenceSubstitutions.Apply(newSubstitutions, previous);
        }
    }

    private static InferenceConstraint Apply(InferenceConstraint constraint, IReadOnlyDictionary<int, StackyType> substitutions)
    {
        if (!substitutions.Any())
        {
            return constraint;
        }

        return new InferenceConstraint(
            InferenceSubstitutions.Apply(substitutions, constraint.Left), 
            InferenceSubstitutions.Apply(substitutions, constraint.Right));
    }

    private static (IReadOnlyDictionary<int, StackyType> Substitutions, IReadOnlyList<InferenceConstraint> Constraints) Solve(InferenceConstraint constraint)
    {
        if (constraint.Left == constraint.Right)
        {
            return (new Dictionary<int, StackyType>(), Array.Empty<InferenceConstraint>());
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

        throw new InvalidCastTypeInferenceException(constraint.Left, constraint.Right);
    }
    
    private static (IReadOnlyDictionary<int, StackyType> Substitutions, IReadOnlyList<InferenceConstraint> Constraints) Replace(StackyType.Variable variable, StackyType type)
    {
        if (type is StackyType.Variable typeVariable)
        {
            var mergedSorts = new StackyType.Variable(typeVariable.Id, StackySort.MakeComposite(variable.Sort, typeVariable.Sort));
            
            return (new Dictionary<int, StackyType> { { variable.Id, mergedSorts } }, Array.Empty<InferenceConstraint>());
        }

        if (!variable.Sort.IsCompatible(type))
        {
            throw new InvalidSortTypeInferenceException(variable.Sort, type);
        }
        
        return (new Dictionary<int, StackyType> { { variable.Id, type } }, Array.Empty<InferenceConstraint>());
    }
    
    private static (IReadOnlyDictionary<int, StackyType> Substitutions, IReadOnlyList<InferenceConstraint> Constraints) SolveComposite(StackyType.Composite left, StackyType.Composite right)
    {
        var leftTypes = left.Types;
        var rightTypes = right.Types;

        if (leftTypes.Count != rightTypes.Count)
        {
            throw new NotSupportedException("this could be possible if a variable in the composite is a composite itself");
        }

        var constraints = leftTypes.Zip(rightTypes).Select(x => new InferenceConstraint(x.First, x.Second)).ToList();
        
        return (new Dictionary<int, StackyType>(), constraints);
    }
    
    private static (IReadOnlyDictionary<int, StackyType> Substitutions, IReadOnlyList<InferenceConstraint> Constraints) SolveFunction(StackyType.Function left, StackyType.Function right)
    {
        var constraints = new[]
        {
            new InferenceConstraint(left.Input, right.Input),
            new InferenceConstraint(left.Output, right.Output),
        };
        
        return (new Dictionary<int, StackyType>(), constraints);
    }
    
    private static StackyType FindTypeUsingSort(StackyType.Variable variable)
    {
        var i64 = new StackyType.Integer(true, SyntaxType.IntegerSize.S64);

        if (variable.Sort.IsCompatible(i64))
        {
            return i64;
        }
        
        throw new AmbiguousTypeInferenceException(variable);
    }
}