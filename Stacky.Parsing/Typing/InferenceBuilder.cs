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
        var type = body.Type as StackyType.Function ?? new StackyType.Function(Array.Empty<StackyType>(), new[] { body.Type });
        
        if (function.Input.Count != type.Input.Count)
        {
            throw new Exception("different number of input types");
        }
        if (function.Output.Count != type.Output.Count)
        {
            throw new Exception("different number of output types");
        }
        
        for (var i = 0; i < function.Input.Count; i++)
        {
            state = state.Unify(ToType(function.Input[i]), type.Input[i]);
        }
        for (var i = 0; i < function.Output.Count; i++)
        {
            state = state.Unify(ToType(function.Output[i]), type.Output[i]);
        }

        return new TypedFunction(function, type, body);
    }

    private static StackyType ToType(SyntaxType type)
    {
        return type switch
        {
            SyntaxType.Boolean => new StackyType.Boolean(),
            SyntaxType.Integer integer => new StackyType.Integer(integer.Signed, integer.Size),
            SyntaxType.String => new StackyType.String(),
            
            SyntaxType.Function function => new StackyType.Function(function.Input.Select(ToType).ToList(), function.Output.Select(ToType).ToList()),
            
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
            SyntaxExpression.Function function => throw new NotImplementedException(),

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

        var input = new Stack<StackyType>();
        var output = new Stack<StackyType>();
        
        foreach (var typed in expressions)
        {
            if (typed.Type is StackyType.Function func)
            {
                foreach (var funcInput in func.Input)
                {
                    if (output.Count > 0)
                    {
                        var inputType = output.Pop();
                        state = state.Unify(funcInput, inputType);
                    }
                    else
                    {
                        input.Push(funcInput);
                    }
                }

                foreach (var funcOutput in func.Output)
                {
                    output.Push(funcOutput);
                }
            }
            else
            {
                output.Push(typed.Type);
            }
        }
        
        var type = new StackyType.Function(input.ToList(), output.ToList());
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

        var functionType = new StackyType.Function(
            function.Input.Select(ToType).ToList(),
            function.Output.Select(ToType).ToList());
        
        return new TypedExpression.Identifier(identifier, functionType);
    }
}