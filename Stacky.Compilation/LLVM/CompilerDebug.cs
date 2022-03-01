using LLVMSharp;
using LLVMSharp.Interop;
using Stacky.Parsing.Syntax;
using Stacky.Parsing.Typing;

namespace Stacky.Compilation.LLVM;

public class CompilerDebug
{
    private readonly LLVMContext _context;
    private readonly IRBuilder _builder;

    private LLVMDIBuilderRef _di;
    
    private readonly LLVMMetadataRef _cu;
    private readonly LLVMMetadataRef _file;

    private readonly Stack<LLVMMetadataRef> _scopes = new();
    
    public CompilerDebug(LLVMContext context, LLVMModuleRef module, IRBuilder builder)
    {
        _context = context;
        _builder = builder;
        _di = module.CreateDIBuilder();

        _file = _di.CreateFile("<unknown>", "");
        _cu = _di.CreateCompileUnit(LLVMDWARFSourceLanguage.LLVMDWARFSourceLanguageC, _file, "Stacky Compiler", 0, "", 0, "", LLVMDWARFEmissionKind.LLVMDWARFEmissionFull, 0, 0, 0, "", "");
        _scopes.Push(_cu);
    }

    public void EmitLocation(SyntaxExpression expression)
    {
        var position = expression.Position;
        
        var location = _context.Handle.CreateDebugLocation((uint)position.Start.Line, (uint)position.Start.LineOffset, _scopes.Peek(), null);
        _builder.Handle.SetCurrentDebugLocation(location);
    }
    
    public void SetFunctionLocation(CompilerValue definition, DebugLocation debugLocation)
    {
        var file = _di.CreateFile(debugLocation.Position.Start.File ?? "<unknown>", "");
        var scope = _cu;

        var line = (uint)debugLocation.Position.Start.Line;
        var voidType = _di.CreateBasicType("void", 0, 0);
        var functionType = _di.CreateSubroutineType(file, new LLVMMetadataRef[]
        {
            voidType
        }, LLVMDIFlags.LLVMDIFlagZero);
        
        var subProgram = _di.CreateFunction(scope, definition.Value.Handle.Name, definition.Value.Handle.Name, file, line, functionType, 0, 1, line, LLVMDIFlags.LLVMDIFlagZero, 0);
        
        _scopes.Push(subProgram);
        
        definition.Value.Handle.SetSubprogram(subProgram);
    }

    public void InitOutput()
    {
        _di.DIBuilderFinalize();
    }
}

public record DebugLocation(SyntaxPosition Position);