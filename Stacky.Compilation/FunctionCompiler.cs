using System.Collections;
using LLVMSharp;
using Stacky.Compilation.LLVM;
using Stacky.Parsing.Syntax;

namespace Stacky.Compilation;

public class FunctionCompiler
{
    private readonly SyntaxFunction _function;
    private readonly CompilerAllocator _allocator;
    private readonly CompilerEnvironment _environment;
    private readonly CompilerEmitter _emitter;
    private readonly CompilerIntrinsics _intrinsics;

    public FunctionCompiler(SyntaxFunction function, CompilerAllocator allocator, CompilerEnvironment environment, CompilerEmitter emitter, CompilerIntrinsics intrinsics)
    {
        _function = function;

        _allocator = allocator;
        _environment = environment;
        _emitter = emitter;
        _intrinsics = intrinsics;
    }

    public void Compile()
    {
        var anonymousFunctions = CompileAnonymousFunctions(_function.Body);

        var definition = _environment.GetFunction(_function.Name.Value);
        CompileFunction(definition, _function.Body, anonymousFunctions);
    }

    private void CompileFunction(CompilerValue definition, SyntaxExpression body, IReadOnlyDictionary<SyntaxExpression.Function, CompilerValue> anonymousFunctionsMapping)
    {
        var stack = new CompilerStack(_allocator, _emitter);
        var type = (CompilerType.Function)definition.Type;

        foreach (var input in type.Inputs)
        {
            stack = stack.PushType(input);
        }

        var compiler = new ExpressionCompiler(_emitter, _environment, _intrinsics, anonymousFunctionsMapping);

        var entry = _emitter.CreateBlock(definition, "entry");
        _emitter.BeginBlock(entry);
        
        stack = compiler.Compile(stack, body);

        _emitter.RetVoid();
        _emitter.VerifyFunction(definition);
    }

    private IReadOnlyDictionary<SyntaxExpression.Function, CompilerValue> CompileAnonymousFunctions(SyntaxExpression expression)
    {
        var mapping = new Dictionary<SyntaxExpression.Function, CompilerValue>();

        var functions = ExtractAnonymousFunctions(expression);

        var i = 0;
        foreach (var function in functions)
        {
            // TODO this is hard coded for now until type inference is a thing (soon I promise!)
            CompilerType.Function type;
            if (function.Body is SyntaxExpression.Application app && app.Expressions.Last() is SyntaxExpression.Identifier id && id.Value == ">")
            {
                type = new CompilerType.Function(new[] { new CompilerType.Long() }, new CompilerType[] { new CompilerType.Long(), new CompilerType.Boolean() });
            }
            else
            {
                type = new CompilerType.Function(new[] { new CompilerType.Long() }, new[] { new CompilerType.Long() });
            }
            

            var definition = _environment.DefineFunction($"__{_function.Name.Value}_anon<{i++}>", type);
            CompileFunction(definition, function.Body, mapping);
            
            mapping.Add(function, definition);
        }
        
        return mapping;
    }

    private static IEnumerable<SyntaxExpression.Function> ExtractAnonymousFunctions(SyntaxExpression expression)
    {
        return expression switch
        {
            SyntaxExpression.LiteralInteger literalInteger => Array.Empty<SyntaxExpression.Function>(),
            SyntaxExpression.LiteralString literalString => Array.Empty<SyntaxExpression.Function>(),
            SyntaxExpression.Identifier identifier => Array.Empty<SyntaxExpression.Function>(),

            SyntaxExpression.Function function => ExtractAnonymousFunctions(function.Body).Append(function),
            SyntaxExpression.Application application => application.Expressions.SelectMany(ExtractAnonymousFunctions),

            _ => throw new ArgumentOutOfRangeException(nameof(expression))
        };
    }
}