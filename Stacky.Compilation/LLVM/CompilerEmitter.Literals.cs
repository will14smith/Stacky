using LLVMSharp;
using LLVMSharp.Interop;
using Stacky.Parsing.Syntax;
using Stacky.Parsing.Typing;

namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    public CompilerValue Literal(string literal)
    {
        var value = _builder.CreateGlobalStringPtr(literal, "str");
        // TODO this should be copied from global -> gc heap
        return new CompilerValue(value, new CompilerType.String());
    }

    public CompilerValue Literal(TypedExpression.LiteralInteger literal)
    {
        if (literal.Type.LastOutput() is not StackyType.Integer intType)
        {
            throw new InvalidCastException();
        }

        if (intType.Signed != true || intType.Size != SyntaxType.IntegerSize.S64)
        {
            throw new NotImplementedException("TODO support other int sizes");
        }

        var type = _context.Handle.Int64Type;
        var value = LLVMValueRef.CreateConstInt(type, (ulong)literal.Value, true);
        
        return new CompilerValue(value.AsValue(), new CompilerType.Long());
    }
    public CompilerValue Literal(long literal)
    {
        var type = _context.Handle.Int64Type;
        var value = LLVMValueRef.CreateConstInt(type, (ulong)literal, true);
        
        return new CompilerValue(value.AsValue(), new CompilerType.Long());
    }
    
    public CompilerValue Literal(bool literal)
    {
        var type = _context.Handle.Int1Type;
        var value = LLVMValueRef.CreateConstInt(type, literal ? 1u : 0u, true);
        
        return new CompilerValue(value.AsValue(), new CompilerType.Boolean());
    }
}