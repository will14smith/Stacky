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
            var originalConstraint = constraints.Pop();
            var constraint = Apply(originalConstraint, substitutions);

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
                var substitution = FindTypeUsingSort(remainingVariable);
                
                UpdateSubstitutions(substitutions, new Dictionary<int, StackyType> { { variable.Id, substitution } });
                substitutions[variable.Id] = substitution;
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
        if (Occurs(variable, type))
        {
            throw new InvalidOccurenceInferenceException(variable, type);
        }
        
        if (type is StackyType.Variable typeVariable)
        {
            var mergedSorts = new StackyType.Variable(typeVariable.Id, StackySort.MakeComposite(variable.Sort, typeVariable.Sort));
            
            return (new Dictionary<int, StackyType>
            {
                { variable.Id, mergedSorts },
                { typeVariable.Id, mergedSorts },
            }, Array.Empty<InferenceConstraint>());
        }

        if (!variable.Sort.IsCompatible(type))
        {
            throw new InvalidSortTypeInferenceException(variable.Sort, type);
        }

        return (new Dictionary<int, StackyType> { { variable.Id, type } }, Array.Empty<InferenceConstraint>());
    }

    private static bool Occurs(StackyType.Variable variable, StackyType type)
    {
        return type switch
        {
            StackyType.Boolean => false,
            StackyType.Integer => false,
            StackyType.String => false,
            StackyType.Void => false,

            StackyType.Variable v => v == variable,

            StackyType.Composite composite => Occurs(variable, composite.Left) || Occurs(variable, composite.Right),
            StackyType.Function function => Occurs(variable, function.Input) || Occurs(variable, function.Output),
            StackyType.Getter getter => Occurs(variable, getter.StructType),
            StackyType.Setter setter => Occurs(variable, setter.StructType),
            StackyType.Struct @struct => @struct.Fields.Any(field => Occurs(variable, field.Type)),
            
            _ => false
        };
    }

    private static (IReadOnlyDictionary<int, StackyType> Substitutions, IReadOnlyList<InferenceConstraint> Constraints) SolveComposite(StackyType.Composite left, StackyType.Composite right)
    {
        return (new Dictionary<int, StackyType>(), new[]
        {
            new InferenceConstraint(left.Right, right.Right),
            new InferenceConstraint(left.Left, right.Left),
        });
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
        if (variable.Sort is StackySort.Stack)
        {
            return new StackyType.Void();
        }
        
        var i64 = new StackyType.Integer(true, SyntaxType.IntegerSize.S64);

        if (variable.Sort.IsCompatible(i64))
        {
            return i64;
        }
        
        throw new AmbiguousTypeInferenceException(variable);
    }
}