using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public class InferenceBuilder
{
    public static (TypedProgram, InferenceState) Build(SyntaxProgram program)
    {
        var state = new InferenceState(program);

        var functions = new List<TypedFunction>();
        foreach (var function in program.Functions)
        {
            functions.Add(Build(ref state, function));
        }
        
        var typed = new TypedProgram(program, functions);
        return (typed, state);
    }

    private static TypedFunction Build(ref InferenceState state, SyntaxFunction function)
    {
        var body = Build(ref state, function.Body);
        var type = body.Type as StackyType.Function ?? new StackyType.Function(new StackyType.Void(), body.Type);

        var funcType = ToType(function);

        state = state
            .Unify(funcType.Input, type.Input)
            .Unify(funcType.Output, type.Output);

        return new TypedFunction(function, type, body);
    }

    private static StackyType.Function ToType(SyntaxFunction function)
    {
        var input = StackyType.MakeComposite(function.Input.Select(ToType).ToArray());
        var output = StackyType.MakeComposite(function.Output.Select(ToType).ToArray());
        
        return new StackyType.Function(input, output);
    }

    private static StackyType ToType(SyntaxType type)
    {
        return type switch
        {
            SyntaxType.Boolean => new StackyType.Boolean(),
            SyntaxType.Integer integer => new StackyType.Integer(integer.Signed, integer.Size),
            SyntaxType.String => new StackyType.String(),
            
            SyntaxType.Function function => new StackyType.Function(
                StackyType.MakeComposite(function.Input.Select(ToType).ToArray()), 
                StackyType.MakeComposite(function.Output.Select(ToType).ToArray())),
            
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private static TypedExpression Build(ref InferenceState state, SyntaxExpression expression)
    {
        return expression switch
        {
            SyntaxExpression.LiteralInteger literal => BuildLiteral(ref state, literal),
            SyntaxExpression.LiteralString literal => BuildLiteral(ref state, literal),

            SyntaxExpression.Application application => BuildApplication(ref state, application),
            SyntaxExpression.Identifier identifier => BuildIdentifier(ref state, identifier),
            SyntaxExpression.Function function => BuildFunction(ref state, function),

            _ => throw new ArgumentOutOfRangeException(nameof(expression))
        };
    }
    
    private static TypedExpression BuildLiteral(ref InferenceState state, SyntaxExpression.LiteralInteger literal)
    {
        // TODO sort could be more specific depending on value
        state = state.NewVariable(new StackySort.Numeric(), out var type);

        return new TypedExpression.LiteralInteger(literal, type);
    }
    private static TypedExpression BuildLiteral(ref InferenceState state, SyntaxExpression.LiteralString literal) => new TypedExpression.LiteralString(literal, new StackyType.String());
    
    private static TypedExpression BuildApplication(ref InferenceState state, SyntaxExpression.Application application)
    {
        var expressions = new List<TypedExpression>();
        foreach (var expression in application.Expressions)
        {
            var typed = Build(ref state, expression);
            expressions.Add(typed);
        }

        StackyType input = new StackyType.Void();
        StackyType output = new StackyType.Void();

        foreach (var typed in expressions)
        {
            if (typed.Type is StackyType.Function function)
            {
                input = Apply(ref state, function.Input, output, input);
                output = Apply(ref state, output, function.Input, function.Output);
            }
            else
            {
                output = StackyType.MakeComposite(output, typed.Type);
            }
        }
        
        var type = new StackyType.Function(input, output);

        return new TypedExpression.Application(application, type, expressions);
    }
    
    private static TypedExpression BuildIdentifier(ref InferenceState state, SyntaxExpression.Identifier identifier)
    {
        if (InferenceIntrinsics.TryInfer(identifier.Value, out var handler))
        {
            state = handler!(state, out var type);
            
            return new TypedExpression.Identifier(identifier, type);
        }

        var function = state.Program.Functions.FirstOrDefault(x => x.Name.Value == identifier.Value);
        if (function == null)
        {
            throw new Exception($"unknown identifier: {identifier}");
        }
        
        var functionType = ToType(function);
        return new TypedExpression.Identifier(identifier, functionType);
    }
    
    private static TypedExpression BuildFunction(ref InferenceState state, SyntaxExpression.Function function)
    {
        var expr = Build(ref state, function.Body);

        StackyType type;
        if (expr.Type is StackyType.Function func)
        {
            type = func;
        }
        else
        {
            type = new StackyType.Function(new StackyType.Void(), expr.Type);
        }

        // since this is a function value, rather than a direct invokable we need to wrap it
        var funcType = new StackyType.Function(new StackyType.Void(), type);
        
        return new TypedExpression.Function(function, funcType, expr);
    }
    
    public static StackyType Apply(ref InferenceState state, StackyType baseType, StackyType remove, StackyType add)
    {
        return Add(ref state, Remove(ref state, baseType, remove), add);
    }

    private static StackyType Remove(ref InferenceState state, StackyType type, StackyType remove)
    {
        var types = StackyType.Iterator(type).ToList();
        var removeTypes = StackyType.Iterator(remove).ToList();

        while (types.Count > 0 && removeTypes.Count > 0)
        {
            state = state.Unify(types[^1], removeTypes[^1]);
            
            types.RemoveAt(types.Count - 1);
            removeTypes.RemoveAt(removeTypes.Count - 1);
        }
        
        return StackyType.MakeComposite(types.ToArray());
    }

    private static StackyType Add(ref InferenceState state, StackyType type, StackyType add)
    {
        var types = StackyType.Iterator(type).ToList();
        var addTypes = StackyType.Iterator(add).ToList();
        
        types.AddRange(addTypes);
        
        return StackyType.MakeComposite(types.ToArray());
    }
}