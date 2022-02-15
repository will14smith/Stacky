using System.Collections;
using LLVMSharp;
using Stacky.Compilation.LLVM;
using Stacky.Parsing.Syntax;
using Stacky.Parsing.Typing;

namespace Stacky.Compilation;

public class FunctionCompiler
{
    private readonly TypedFunction _function;
    private readonly CompilerAllocator _allocator;
    private readonly CompilerEnvironment _environment;
    private readonly CompilerEmitter _emitter;
    private readonly CompilerIntrinsics _intrinsics;
    private readonly CompilerTypeBuilder _typeBuilder;

    public FunctionCompiler(TypedFunction function, CompilerAllocator allocator, CompilerEnvironment environment, CompilerEmitter emitter, CompilerIntrinsics intrinsics, CompilerTypeBuilder typeBuilder)
    {
        _function = function;

        _allocator = allocator;
        _environment = environment;
        _emitter = emitter;
        _intrinsics = intrinsics;
        _typeBuilder = typeBuilder;
    }

    public void Compile()
    {
        var anonymousFunctions = CompileAnonymousFunctions(_function.Body);

        var definition = _environment.GetFunction(_function.Name.Value);
        CompileFunction(definition, _function.Body, anonymousFunctions);
    }

    private void CompileFunction(CompilerValue definition, TypedExpression body, IReadOnlyDictionary<TypedExpression.Function, CompilerValue> anonymousFunctionsMapping)
    {
        var stack = new CompilerStack(_allocator, _emitter);
        var type = (CompilerType.Function)definition.Type;

        foreach (var input in type.Inputs)
        {
            stack = stack.PushType(input);
        }

        var compiler = new ExpressionCompiler(_allocator, _emitter, _environment, _intrinsics, anonymousFunctionsMapping);

        var entry = _emitter.CreateBlock(definition, "entry");
        _emitter.BeginBlock(entry);
        
        stack = compiler.Compile(stack, body);

        _emitter.RetVoid();
        _emitter.VerifyFunction(definition);
    }

    private IReadOnlyDictionary<TypedExpression.Function, CompilerValue> CompileAnonymousFunctions(TypedExpression expression)
    {
        var mapping = new Dictionary<TypedExpression.Function, CompilerValue>();

        var functions = ExtractAnonymousFunctions(expression);

        var i = 0;
        foreach (var function in functions)
        {
            var type = _typeBuilder.BuildFunction((StackyType.Function)function.Type);
            if (type.Inputs.Count != 0) { throw new InvalidOperationException(); }
            if (type.Outputs.Count != 1) { throw new InvalidOperationException(); }

            var functionType = (CompilerType.Function) type.Outputs[0]; 
            
            var definition = _environment.DefineFunction($"__{_function.Name.Value}_anon<{i++}>", functionType);
            CompileFunction(definition, function.Body, mapping);
            
            mapping.Add(function, definition);
        }
        
        return mapping;
    }

    private static IEnumerable<TypedExpression.Function> ExtractAnonymousFunctions(TypedExpression expression)
    {
        return expression switch
        {
            TypedExpression.LiteralInteger literalInteger => Array.Empty<TypedExpression.Function>(),
            TypedExpression.LiteralString literalString => Array.Empty<TypedExpression.Function>(),
            TypedExpression.Identifier identifier => Array.Empty<TypedExpression.Function>(),

            TypedExpression.Function function => ExtractAnonymousFunctions(function.Body).Append(function),
            TypedExpression.Application application => application.Expressions.SelectMany(ExtractAnonymousFunctions),

            _ => throw new ArgumentOutOfRangeException(nameof(expression))
        };
    }
}