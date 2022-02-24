using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public class InferenceBuilder
{
    private readonly InferenceIntrinsicRegistry _intrinsicRegistry;

    public InferenceBuilder(InferenceIntrinsicRegistry intrinsicRegistry)
    {
        _intrinsicRegistry = intrinsicRegistry;
    }

    public (TypedProgram, InferenceState) Build(SyntaxProgram program)
    {
        var state = new InferenceState(program);

        var structs = new List<TypedStruct>();
        foreach (var definition in program.Structs)
        {
            structs.Add(Build(ref state, definition));
        }
        
        var functions = new List<TypedFunction>();
        foreach (var function in program.Functions)
        {
            functions.Add(Build(ref state, function));
        }
        
        var typed = new TypedProgram(program, functions, structs);
        return (typed, state);
    }

    private static TypedStruct Build(ref InferenceState state, SyntaxStruct definition)
    {
        var fields = new List<TypedStructField>();
        foreach (var f in definition.Fields)
        {
            fields.Add(Build(ref state, f));
        }

        var typeFields = fields.Select(f => new StackyType.StructField(f.Name.Value, f.Type)).ToList();
        var type = new StackyType.Struct(definition.Name.Value, typeFields);
        
        return new TypedStruct(definition, type, fields);
    }

    private static TypedStructField Build(ref InferenceState state, SyntaxStructField field)
    {
        var type = ToType(ref state, field.Type);
        return new TypedStructField(field, type);
    }

    private TypedFunction Build(ref InferenceState state, SyntaxFunction function)
    {
        var body = Build(ref state, function.Body);
        var type = (StackyType.Function)body.Type;
        
        var funcType = ToDefinitionType(ref state, function);

        state = state
            .Unify(funcType.Input, type.Input)
            .Unify(funcType.Output, type.Output);

        return new TypedFunction(function, funcType, body);
    }

    private static StackyType.Function ToDefinitionType(ref InferenceState state, SyntaxFunction function)
    {
        var localState = state;
        var input = StackyType.MakeComposite(function.Input.Select(t => ToType(ref localState, t)).Prepend(new StackyType.Void()).ToArray());
        var output = StackyType.MakeComposite(function.Output.Select(t => ToType(ref localState, t)).Prepend(new StackyType.Void()).ToArray());
        state = localState;

        return new StackyType.Function(input, output);
    }
    
    private static StackyType.Function ToInvokeType(ref InferenceState state, SyntaxFunction function)
    {
        state = state.NewStackVariable(out var inputStack);

        var localState = state;
        var input = StackyType.MakeComposite(function.Input.Select(t => ToType(ref localState, t)).Prepend(inputStack).ToArray());
        var output = StackyType.MakeComposite(function.Output.Select(t => ToType(ref localState, t)).Prepend(inputStack).ToArray());
        state = localState;

        return new StackyType.Function(input, output);
    }
    
    private static StackyType ToInvokeType(ref InferenceState state, SyntaxType.Function function)
    {
        state = state.NewStackVariable(out var inputStack);

        var localState = state;
        var input = StackyType.MakeComposite(function.Input.Select(t => ToType(ref localState, t)).Prepend(inputStack).ToArray());
        var output = StackyType.MakeComposite(function.Output.Select(t => ToType(ref localState, t)).Prepend(inputStack).ToArray());
        state = localState;
        
        return new StackyType.Function(input, output);
    }
    
    private static StackyType ToType(ref InferenceState state, SyntaxStruct structDef)
    {
        var localState = state;
        var fields = structDef.Fields.Select(x => new StackyType.StructField(x.Name.Value, ToType(ref localState, x.Type))).ToList();
        state = localState;
        
        return new StackyType.Struct(structDef.Name.Value, fields);
    }


    private static StackyType ToType(ref InferenceState state, SyntaxType type)
    {
        return type switch
        {
            SyntaxType.Boolean => new StackyType.Boolean(),
            SyntaxType.Integer integer => new StackyType.Integer(integer.Signed, integer.Size),
            SyntaxType.String => new StackyType.String(),
            
            SyntaxType.Function function => ToInvokeType(ref state, function),
            
            SyntaxType.Struct structRef => ToType(ref state, state.LookupStruct(structRef.Name)),
            
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
    
    private TypedExpression Build(ref InferenceState state, SyntaxExpression expression)
    {
        return expression switch
        {
            SyntaxExpression.LiteralInteger literal => BuildLiteral(ref state, literal),
            SyntaxExpression.LiteralString literal => BuildLiteral(ref state, literal),

            SyntaxExpression.Application application => BuildApplication(ref state, application),
            SyntaxExpression.Identifier identifier => BuildIdentifier(ref state, identifier),
            SyntaxExpression.Function function => BuildFunction(ref state, function),
            SyntaxExpression.Binding binding => BuildBinding(ref state, binding),

            _ => throw new ArgumentOutOfRangeException(nameof(expression))
        };
    }
    
    private static TypedExpression BuildLiteral(ref InferenceState state, SyntaxExpression.LiteralInteger literal)
    {
        state = state.NewStackVariable(out var stack);
        // TODO sort could be more specific depending on value
        state = state.NewVariable(new StackySort.Numeric(), out var value);

        var type = new StackyType.Function(stack, StackyType.MakeComposite(stack, value));
        
        return new TypedExpression.LiteralInteger(literal, type);
    }
    private static TypedExpression BuildLiteral(ref InferenceState state, SyntaxExpression.LiteralString literal)
    {
        state = state.NewStackVariable(out var stack);
        var type = new StackyType.Function(stack, StackyType.MakeComposite(stack, new StackyType.String()));
        
        return new TypedExpression.LiteralString(literal, type);
    }

    private TypedExpression BuildApplication(ref InferenceState state, SyntaxExpression.Application application)
    {
        var expressions = new List<TypedExpression>();
        foreach (var expression in application.Expressions)
        {
            var typed = Build(ref state, expression);
            expressions.Add(typed);
        }

        if (expressions.Count == 0)
        {
            state = state.NewStackVariable(out var id);
            return new TypedExpression.Application(application, new StackyType.Function(id, id), expressions);
        }
        
        var firstType = (StackyType.Function) expressions[0].Type;

        var input = firstType.Input;
        var output = firstType.Output;
        
        for (var i = 1; i < expressions.Count; i++)
        {
            var expressionType = (StackyType.Function) expressions[i].Type;
            
            state = state.Unify(output, expressionType.Input);
            output = expressionType.Output;
        }
        
        var type = new StackyType.Function(input, output);
        return new TypedExpression.Application(application, type, expressions);
    }
    
    private TypedExpression BuildIdentifier(ref InferenceState state, SyntaxExpression.Identifier identifier)
    {
        if (state.TryLookupBinding(identifier.Value, out var binding))
        {
            state = state.NewStackVariable(out var stack);
            var type = new StackyType.Function(stack, StackyType.MakeComposite(stack, binding!));
            return new TypedExpression.Identifier(identifier, type);
        }
        
        if (identifier.Value.Length > 1)
        {
            switch (identifier.Value[0])
            {
                case '@': return BuildInit(ref state, identifier);                
                case '#': return BuildGetter(ref state, identifier);                
                case '~': return BuildSetter(ref state, identifier);                
            }
        }
        
        if (_intrinsicRegistry.TryGetIntrinsic(identifier.Value, out var intrinsic))
        {
            state = intrinsic!.Infer(state, out var type);
            
            return new TypedExpression.Identifier(identifier, type);
        }

        var function = state.Program.Functions.FirstOrDefault(x => x.Name.Value == identifier.Value);
        if (function == null)
        {
            throw new Exception($"unknown identifier: {identifier}");
        }
        
        var functionType = ToInvokeType(ref state, function);
        return new TypedExpression.Identifier(identifier, functionType);
    }

    private static TypedExpression BuildInit(ref InferenceState state, SyntaxExpression.Identifier identifier)
    {
        var structName = identifier.Value[1..];
        var structDef = state.LookupStruct(structName);
        if (structDef == null)
        {
            throw new TypeInferenceException($"Failed to find struct '{structName}' to initialise");
        }

        var structType = ToType(ref state, structDef);

        state = state.NewStackVariable(out var stack);
        var type = new StackyType.Function(stack, StackyType.MakeComposite(stack, structType));        
        return new TypedExpression.Identifier(identifier, type);
    }
    
    private static TypedExpression BuildGetter(ref InferenceState state, SyntaxExpression.Identifier identifier)
    {
        var fieldName = identifier.Value[1..];

        state = state.NewStackVariable(out var stack);
        state = state.NewVariable(new StackySort.Gettable(fieldName), out var structType);

        state = state.NewVariable(new StackySort.Any(), out var output);
        state = state.Unify(output, StackyType.MakeGetter(structType, fieldName));
        
        var type = new StackyType.Function(StackyType.MakeComposite(stack, structType), StackyType.MakeComposite(stack, output));
        
        return new TypedExpression.Identifier(identifier, type);
    }

    private static TypedExpression BuildSetter(ref InferenceState state, SyntaxExpression.Identifier identifier)
    {
        var fieldName = identifier.Value[1..];

        state = state.NewStackVariable(out var stack);
        state = state.NewVariable(new StackySort.Settable(fieldName), out var structType);

        state = state.NewVariable(new StackySort.Any(), out var value);
        state = state.Unify(value, StackyType.MakeSetter(structType, fieldName));
        
        var type = new StackyType.Function(StackyType.MakeComposite(stack, structType, value), StackyType.MakeComposite(stack, structType));
        
        return new TypedExpression.Identifier(identifier, type);
    }

    private TypedExpression BuildFunction(ref InferenceState state, SyntaxExpression.Function function)
    {
        var expr = Build(ref state, function.Body);
        var functionType = (StackyType.Function)expr.Type;

        state = state.NewStackVariable(out var stack); 

        // since this is a function value, rather than a direct invokable we need to wrap it
        var type = new StackyType.Function(stack, StackyType.MakeComposite(stack, functionType));
        return new TypedExpression.Function(function, type, expr);
    }
    
    private TypedExpression BuildBinding(ref InferenceState state, SyntaxExpression.Binding binding)
    {
        state = state.NewStackVariable(out var stack);
        var inputs = new List<StackyType> { stack };
        var nameExpressions = new List<TypedExpression.Identifier>();
        var names = new Dictionary<string, StackyType>();
        foreach (var name in binding.Names)
        {
            state = state.NewVariable(new StackySort.Any(), out var nameType);
            
            nameExpressions.Add(new TypedExpression.Identifier(name, nameType));
            names[name.Value] = nameType;
            inputs.Add(nameType);
        }
        
        var removeBindings = new StackyType.Function(StackyType.MakeComposite(inputs.ToArray()), StackyType.MakeComposite(stack));

        state = state.PushBindings(names);

        var body = Build(ref state, binding.Body);
        var bodyType = (StackyType.Function)body.Type;
        
        state = state.PopBindings();
        state = state.Unify(removeBindings.Output, bodyType.Input);        
        
        var type = new StackyType.Function(
            removeBindings.Input,
            bodyType.Output
        );
        
        return new TypedExpression.Binding(binding, type, nameExpressions, body);
    }
}