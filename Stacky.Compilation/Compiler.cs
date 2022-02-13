using Stacky.Compilation.LLVM;
using Stacky.Parsing.Syntax;
using Stacky.Parsing.Typing;

namespace Stacky.Compilation;

public class Compiler
{
    private readonly TypedProgram _program;

    private readonly CompilerEmitter _emitter;
    private readonly CompilerEnvironment _environment;
    private readonly CompilerTypeBuilder _typeBuilder;
    private readonly CompilerAllocator _allocator;
    private readonly CompilerIntrinsics _intrinsics;

    public Compiler(TypedProgram program)
    {
        _program = program;

        _emitter = new CompilerEmitter();
        _environment = new CompilerEnvironment(_emitter);
        _typeBuilder = new CompilerTypeBuilder();
        _allocator = new CompilerAllocator(_emitter);
        _intrinsics = new CompilerIntrinsics(_allocator, _emitter);
    }

    public void Compile()
    {
        foreach (var function in _program.Functions)
        {
            _environment.DefineFunction(function.Name.Value, _typeBuilder.BuildFunction(function.Type));
        }
        
        foreach (var function in _program.Functions)
        {
            var functionCompiler = new FunctionCompiler(function, _allocator, _environment, _emitter, _intrinsics, _typeBuilder);
            functionCompiler.Compile();
        }

        _emitter.OutputAssembly("output.asm");
        _emitter.OutputObject("output.o");
    }
}