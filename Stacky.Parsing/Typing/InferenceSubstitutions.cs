using System.Linq;

namespace Stacky.Parsing.Typing;

public class InferenceSubstitutions
{
    private readonly IReadOnlyDictionary<StackyType.Variable, StackyType> _substitutions;

    public InferenceSubstitutions(IReadOnlyDictionary<StackyType.Variable, StackyType> substitutions)
    {
        _substitutions = substitutions;
    }

    public TypedProgram Apply(TypedProgram built)
    {
        if (!_substitutions.Any())
        {
            return built;
        }

        var functions = built.Functions.Select(Apply).ToList();
        
        return new TypedProgram(built.Syntax, functions);
    }

    private TypedFunction Apply(TypedFunction function)
    {
        var type = (StackyType.Function) Apply(_substitutions, function.Type);
        
        return new TypedFunction(function.Syntax, type, Apply(function.Body));
    }

    private TypedExpression Apply(TypedExpression expression)
    {
        return expression switch
        {
            TypedExpression.LiteralInteger literal => new TypedExpression.LiteralInteger(literal.Syntax, Apply(_substitutions, literal.Type)),
            TypedExpression.LiteralString literal => new TypedExpression.LiteralString(literal.Syntax, Apply(_substitutions, literal.Type)),

            TypedExpression.Application application => ApplyApplication(application),
            TypedExpression.Identifier identifier => new TypedExpression.Identifier(identifier.Syntax, Apply(_substitutions, identifier.Type)),

            TypedExpression.Function function => new TypedExpression.Function(function.Syntax, Apply(_substitutions, function.Type)),

            _ => throw new ArgumentOutOfRangeException(nameof(expression))
        };
    }

    private TypedExpression ApplyApplication(TypedExpression.Application application)
    {
        var type = Apply(_substitutions, application.Type);
        var expressions = application.Expressions.Select(Apply).ToList();
        
        return new TypedExpression.Application(application.Syntax, type, expressions);
    }

    internal static StackyType Apply(IReadOnlyDictionary<StackyType.Variable, StackyType> substitutions, StackyType type)
    {
        if (type is StackyType.Variable variable)
        {
            if (substitutions.TryGetValue(variable, out var variableReplacement))
            {
                return variableReplacement;
            }
        }

        if (type is StackyType.Function function)
        {
            return new StackyType.Function(Apply(substitutions, function.Input), Apply(substitutions, function.Output));
        }

        if (type is StackyType.Composite composite)
        {
            var types = composite.Types.Select(t => Apply(substitutions, t)).ToArray();
            return StackyType.MakeComposite(types);
        }
        
        return type;
    }
}