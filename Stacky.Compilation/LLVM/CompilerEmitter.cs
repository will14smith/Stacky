using LLVMSharp;
using LLVMSharp.Interop;
using L = LLVMSharp.Interop.LLVM;

namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    private readonly LLVMContext _context;
    private LLVMModuleRef _module;
    private readonly IRBuilder _builder;

    private readonly LLVMTypeBuilder _types;
    
    public NativeFunctions NativeFunctions { get; }
    
    public CompilerEmitter()
    {
        _context = new LLVMContext();
        
        _module = _context.Handle.CreateModuleWithName("program");

        _builder = new IRBuilder(_context);
        
        _types = new LLVMTypeBuilder(_context);
        NativeFunctions = new NativeFunctions(_context);

        _stackPointer = CreateStack();
    }
}