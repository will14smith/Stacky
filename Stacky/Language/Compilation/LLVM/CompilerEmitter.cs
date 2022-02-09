using LLVMSharp;
using L = LLVMSharp.LLVM;

namespace Stacky.Language.Compilation.LLVM;

public partial class CompilerEmitter
{
    private readonly LLVMContextRef _context;
    private readonly LLVMModuleRef _module;
    private readonly LLVMBuilderRef _builder;

    private readonly LLVMTypeBuilder _types;
    
    public NativeFunctions NativeFunctions { get; }
    
    public CompilerEmitter()
    {
        _context = L.ContextCreate();
        
        _module = L.ModuleCreateWithNameInContext("program", _context);
        _builder = L.CreateBuilderInContext(_context);

        _types = new LLVMTypeBuilder(_context);
        NativeFunctions = new NativeFunctions(_context);

        _stackPointer = CreateStack();
    }
}