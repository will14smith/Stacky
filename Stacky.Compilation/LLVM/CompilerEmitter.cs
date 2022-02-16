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

    public CompilerValue Load(CompilerValue source)
    {
        if (source.Type is not CompilerType.Pointer pointer)
        {
            throw new InvalidOperationException();
        }

        var value = _builder.CreateLoad(source.Value, "value");
        
        return new CompilerValue(value, pointer.Type);
    }
    
    public void Store(CompilerValue dest, CompilerValue value)
    {
        if (dest.Type is not CompilerType.Pointer)
        {
            throw new InvalidOperationException();
        }
        
        _builder.CreateStore(value.Value, dest.Value);
    }
}