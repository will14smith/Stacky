using Stacky.Compilation.LLVM;
using Stacky.Parsing.Typing;

namespace Stacky.Compilation;

public class Compiler
{
    private readonly TypedProgram _program;

    private readonly CompilerEmitter _emitter;
    private readonly CompilerEnvironment _environment;
    private readonly CompilerTypeBuilder _typeBuilder;
    private readonly CompilerAllocator _allocator;
    
    public CompilerIntrinsicRegistry Intrinsics { get; }

    public Compiler(TypedProgram program)
    {
        _program = program;

        _emitter = new CompilerEmitter();
        _environment = new CompilerEnvironment(_emitter);
        _typeBuilder = new CompilerTypeBuilder();
        _allocator = new CompilerAllocator(_emitter);
        Intrinsics = new CompilerIntrinsicRegistry(_allocator, _emitter);
    }

    public void Compile(string outputPath)
    {
        foreach (var definition in _program.Structs)
        {
            _environment.DefineStruct(definition.Name.Value, _typeBuilder.BuildStruct(definition.Type));
        }
        
        foreach (var function in _program.Functions)
        {
            if (function.Name.Value == "main")
            {
                if (function.Type.Input is not StackyType.Void || function.Type.Output is not StackyType.Void)
                {
                    throw new InvalidOperationException();
                }
                
                _environment.DefineMainFunction(function.Name.Value, _typeBuilder.BuildFunction(function.Type), _emitter.NativeFunctions.Main);
            }
            else
            {
                _environment.DefineFunction(function.Name.Value, _typeBuilder.BuildFunction(function.Type));
            }
        }

        foreach (var function in _program.Functions)
        {
            var functionCompiler = new FunctionCompiler(function, _allocator, _environment, _emitter, Intrinsics, _typeBuilder);
            functionCompiler.Compile();
        }
        
        _emitter.OutputAssembly($"{outputPath}.asm");
        _emitter.OutputObject(outputPath);
    }
}