namespace Stacky.Parsing.Typing;

public class InferenceSubstitutions
{
    private readonly IReadOnlyDictionary<int, StackyType> _substitutions;

    public InferenceSubstitutions(IReadOnlyDictionary<int, StackyType> substitutions)
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
        var structs = built.Structs.Select(Apply).ToList();
        
        return new TypedProgram(built.Syntax, functions, structs);
    }
    
    private TypedFunction Apply(TypedFunction function)
    {
        var type = (StackyType.Function) Apply(_substitutions, function.Type);
        
        return new TypedFunction(function.Syntax, type, Apply(function.Body));
    }

    private TypedStruct Apply(TypedStruct definition)
    {
        var type = (StackyType.Struct) Apply(_substitutions, definition.Type);
        var fields = definition.Fields.Select(Apply).ToList();
        
        return new TypedStruct(definition.Syntax, type, fields);
    }
    private TypedStructField Apply(TypedStructField field)
    {
        return new TypedStructField(field.Syntax, Apply(_substitutions, field.Type));
    }

    private TypedExpression Apply(TypedExpression expression)
    {
        return expression switch
        {
            TypedExpression.LiteralInteger literal => new TypedExpression.LiteralInteger(literal.Syntax, Apply(_substitutions, literal.Type)),
            TypedExpression.LiteralString literal => new TypedExpression.LiteralString(literal.Syntax, Apply(_substitutions, literal.Type)),

            TypedExpression.Application application => ApplyApplication(application),
            TypedExpression.Identifier identifier => new TypedExpression.Identifier(identifier.Syntax, Apply(_substitutions, identifier.Type)),

            TypedExpression.Closure closure => new TypedExpression.Closure(closure.Syntax, Apply(_substitutions, closure.Type), Apply(_substitutions, closure.Bindings), Apply(closure.Body)),
            TypedExpression.Binding binding => new TypedExpression.Binding(binding.Syntax, Apply(_substitutions, binding.Type), binding.Names.Select(Apply).Cast<TypedExpression.Identifier>().ToList(), Apply(binding.Body)),

            _ => throw new ArgumentOutOfRangeException(nameof(expression))
        };
    }

    private TypedExpression ApplyApplication(TypedExpression.Application application)
    {
        var type = Apply(_substitutions, application.Type);
        var expressions = application.Expressions.Select(Apply).ToList();
        
        return new TypedExpression.Application(application.Syntax, type, expressions);
    }

    internal static StackyType Apply(IReadOnlyDictionary<int, StackyType> substitutions, StackyType type)
    {
        return type switch
        {
            StackyType.Variable variable when substitutions.TryGetValue(variable.Id, out var variableReplacement) => variableReplacement,
            StackyType.Function function => new StackyType.Function(Apply(substitutions, function.Input), Apply(substitutions, function.Output)),
            StackyType.Composite composite => StackyType.MakeComposite(Apply(substitutions, composite.Left), Apply(substitutions, composite.Right)),
            StackyType.Getter getter => StackyType.MakeGetter(Apply(substitutions, getter.StructType), getter.FieldName),
            StackyType.Setter setter => StackyType.MakeSetter(Apply(substitutions, setter.StructType), setter.FieldName),
            
            _ => type
        };
    }
    
    private static IReadOnlyList<StackyBinding> Apply(IReadOnlyDictionary<int, StackyType> substitutions, IReadOnlyList<StackyBinding> bindings) => bindings.Select(binding => Apply(substitutions, binding)).ToList();
    private static StackyBinding Apply(IReadOnlyDictionary<int, StackyType> substitutions, StackyBinding binding) => new(binding.Name, Apply(substitutions, binding.Type));
}