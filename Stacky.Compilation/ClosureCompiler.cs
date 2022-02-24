using Stacky.Compilation.LLVM;
using Stacky.Parsing.Typing;

namespace Stacky.Compilation;

public class ClosureCompiler
{
    public const string FunctionField = "__<function>";
    public const string StateField = "__<state>";
    
    private readonly TypedFunction _function;
    private readonly CompilerAllocator _allocator;
    private readonly CompilerEnvironment _environment;
    private readonly CompilerEmitter _emitter;
    private readonly CompilerIntrinsicRegistry _intrinsics;
    private readonly CompilerTypeBuilder _typeBuilder;
    
    private int _counter;

    public ClosureCompiler(TypedFunction function, CompilerAllocator allocator, CompilerEnvironment environment, CompilerEmitter emitter, CompilerIntrinsicRegistry intrinsics, CompilerTypeBuilder typeBuilder)
    {
        _function = function;
        _allocator = allocator;
        _environment = environment;
        _emitter = emitter;
        _intrinsics = intrinsics;
        _typeBuilder = typeBuilder;
    }
    
    public IReadOnlyDictionary<TypedExpression.Closure, CompilerClosure> Compile()
    {
        var closures = Extract(_function.Body);

        var mapping = new Dictionary<TypedExpression.Closure, CompilerClosure>();
        foreach (var function in closures)
        {
            var definition = Compile(function, mapping);
            mapping.Add(function, definition);
        }
        
        return mapping;
    }

    private CompilerClosure Compile(TypedExpression.Closure closure, IReadOnlyDictionary<TypedExpression.Closure, CompilerClosure> mapping)
    {
        var name = $"__{_function.Name.Value}_anon<{_counter++}>";

        var stateFields = new List<StackyType.StructField>();
        foreach (var binding in closure.Bindings)
        {
            stateFields.Add(new StackyType.StructField(binding.Name, binding.Type));
        }
        var stateStructType = new StackyType.Struct($"{name}_state", stateFields);
        var stateStruct = _emitter.DefineStruct(stateStructType.Name, _typeBuilder.BuildStruct(stateStructType));
        
        var functionType = (StackyType.Function)closure.Type.LastOutput();
        functionType = new StackyType.Function(StackyType.MakeComposite(functionType.Input, stateStructType), functionType.Output);
        
        var type = _typeBuilder.BuildFunction(functionType);

        var function = _environment.DefineFunction(name, type);
        CompileFunction(function, closure, mapping);

        var closureStructName = $"{name}_closure";
        
        // closureFields.Add(new StackyType.StructField(FunctionField, closure.Type));

        var closureStruct = _emitter.DefineStruct(closureStructName, new CompilerType.Struct(closureStructName, new []
        {
            new CompilerType.StructField(FunctionField, type),
            new CompilerType.StructField(StateField, stateStruct.Type)
        }));
        
        return new CompilerClosure(closureStruct, stateStruct, function);
    }

    private void CompileFunction(CompilerValue definition, TypedExpression.Closure closure, IReadOnlyDictionary<TypedExpression.Closure, CompilerClosure> closures)
    {
        var stack = new CompilerStack(_allocator, _emitter);
        var type = (CompilerType.Function)definition.Type;
        
        foreach (var input in type.Inputs)
        {
            stack = stack.PushType(input);
        }
        
        var compiler = new ExpressionCompiler(_allocator, _emitter, _environment, _intrinsics, closures);

        var entry = _emitter.CreateBlock(definition, "entry");
        _emitter.BeginBlock(entry);

        stack = SetupBindings(stack, definition, out var removeStateRoot);
        stack = compiler.Compile(stack, closure.Body);
        stack = CleanupBindings(stack, removeStateRoot);
        
        _emitter.RetVoid();
        _emitter.VerifyFunction(definition);
    }
    
    private CompilerStack SetupBindings(CompilerStack stack, CompilerValue function, out Action removeStateRoot)
    {
        stack = stack.Pop(out var closureState, out removeStateRoot);
        var closureStruct = (CompilerType.Struct) closureState.Type;
        
        var bindings = new Dictionary<string, CompilerValue>();
        foreach (var field in closureStruct.Fields)
        {
            if (field.Name == FunctionField)
            {
                continue;
            }
            
            var ptr = _emitter.FieldPointer(closureState, field.Name);
            var value = _emitter.Load(ptr);
            
            bindings.Add(field.Name, value);
        }
        
        return stack.PushBindings(bindings);
    }
    
    private CompilerStack CleanupBindings(CompilerStack stack, Action removeStateRoot)
    {
        stack = stack.PopBindings();
        removeStateRoot();
        return stack;
    }

    private static IEnumerable<TypedExpression.Closure> Extract(TypedExpression expression)
    {
        return expression switch
        {
            TypedExpression.LiteralInteger => Array.Empty<TypedExpression.Closure>(),
            TypedExpression.LiteralString => Array.Empty<TypedExpression.Closure>(),
            TypedExpression.Identifier => Array.Empty<TypedExpression.Closure>(),

            TypedExpression.Closure closure => Extract(closure.Body).Append(closure),
            TypedExpression.Application application => application.Expressions.SelectMany(Extract),
            TypedExpression.Binding binding => Extract(binding.Body),

            _ => throw new ArgumentOutOfRangeException(nameof(expression))
        };
    }

}