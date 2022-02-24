using Stacky.Compilation.LLVM;
using Stacky.Parsing.Typing;

namespace Stacky.Compilation;

public partial class ExpressionCompiler
{
    private readonly CompilerAllocator _allocator;
    private readonly CompilerEmitter _emitter;
    private readonly CompilerEnvironment _environment;
    private readonly CompilerIntrinsicRegistry _intrinsics;
    private readonly IReadOnlyDictionary<TypedExpression.Closure, CompilerClosure> _closures;

    public ExpressionCompiler(CompilerAllocator allocator, CompilerEmitter emitter, CompilerEnvironment environment, CompilerIntrinsicRegistry intrinsics, IReadOnlyDictionary<TypedExpression.Closure, CompilerClosure> closures)
    {
        _allocator = allocator;
        _emitter = emitter;
        _environment = environment;
        _intrinsics = intrinsics;
        _closures = closures;
    }

    public CompilerStack Compile(CompilerStack stack, TypedExpression expression)
    {
        return expression switch
        {
            TypedExpression.LiteralInteger literalInteger => CompileLiteral(stack, literalInteger),
            TypedExpression.LiteralString literalString => CompileLiteral(stack, literalString),

            TypedExpression.Closure closure => CompileClosure(stack, closure),
            TypedExpression.Binding binding => CompileBinding(stack, binding),
            
            TypedExpression.Application application => CompileApplication(stack, application),
            TypedExpression.Identifier identifier => CompileIdentifier(stack, identifier),

            _ => throw new ArgumentOutOfRangeException(nameof(expression))
        };
    }
    
    private CompilerStack CompileLiteral(CompilerStack stack, TypedExpression.LiteralInteger literal) => stack.Push(_emitter.Literal(literal));
    private CompilerStack CompileLiteral(CompilerStack stack, TypedExpression.LiteralString literal)
    {
        var value = _allocator.AllocateRaw(new CompilerType.String(), literal.Value.Length + 1);
        _emitter.LiteralInto(value, literal.Value);
        return stack.Push(value);
    }

    private CompilerStack CompileClosure(CompilerStack stack, TypedExpression.Closure closureExpression)
    {
        var closureDefinition = _closures[closureExpression];
        
        var state = _allocator.Allocate(closureDefinition.State);
        state = _emitter.StructInit(state);
        
        foreach (var stateField in closureDefinition.State.Type.Fields)
        {
            if (!stack.TryGetBinding(stateField.Name, out var stateValue))
            {
                throw new InvalidOperationException();
            }
            _emitter.Store(_emitter.FieldPointer(state, stateField.Name), stateValue);
        }
        
        var closure = _allocator.Allocate(closureDefinition.Closure);
        closure = _emitter.StructInit(closure);
        
        _emitter.Store(_emitter.FieldPointer(closure, ClosureCompiler.FunctionField), closureDefinition.Function);
        _emitter.Store(_emitter.FieldPointer(closure, ClosureCompiler.StateField), state);
        
        return stack.Push(closure);
    }

    private CompilerStack CompileBinding(CompilerStack stack, TypedExpression.Binding binding)
    {
        var bindings = new Dictionary<string, CompilerValue>();
        for (var i = binding.Names.Count - 1; i >= 0; i--)
        {
            var name = binding.Names[i];
            stack = stack.Pop(out var value, out var removeRoot);

            if (!bindings.ContainsKey(name.Value))
            {
                bindings[name.Value] = value;
                // re-root as binding
                _allocator.AddRoot(value);
            }
           
            removeRoot();
        }
        
        stack = stack.PushBindings(bindings);
        stack = Compile(stack, binding.Body);
        stack = stack.PopBindings();

        return stack;
    }

    private CompilerStack CompileApplication(CompilerStack stack, TypedExpression.Application application) => application.Expressions.Aggregate(stack, Compile);

    private CompilerStack CompileIdentifier(CompilerStack stack, TypedExpression.Identifier identifier)
    {
        if (stack.TryGetBinding(identifier.Value, out var binding))
        {
            return stack.Push(binding);
        }
        
        if (identifier.Value.Length > 1)
        {
            switch (identifier.Value[0])
            {
                case '@': return CompileInit(stack, identifier);                
                case '#': return CompileGetter(stack, identifier);                
                case '~': return CompileSetter(stack, identifier);                
            }
        }

        if (_intrinsics.TryCompile(identifier.Value, ref stack))
        {
            return stack;
        }

        var function = _environment.GetFunction(identifier.Value);
        return CallFunction(_emitter, stack, function);
    }

    public static CompilerStack CallFunction(CompilerEmitter emitter, CompilerStack stack, CompilerValue function)
    {
        var functionType = (CompilerType.Function)function.Type;

        for (var i = functionType.Inputs.Count - 1; i >= 0; i--)
        {
            stack = stack.PopType(out var stackType);
            var inputType = functionType.Inputs[i];
            if (stackType != inputType)
            {
                throw new InvalidCastException($"unable to cast '{stackType}' to '{inputType}'");
            }
        }

        emitter.CallVoid(function);

        foreach (var outputType in functionType.Outputs)
        {
            stack = stack.PushType(outputType);
        }

        return stack;
    }
}