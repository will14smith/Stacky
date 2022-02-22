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

    public void LiteralInto(CompilerValue target, string literal)
    {
        var value = _builder.CreateGlobalStringPtr(literal, "str");

        _builder.CreateMemCpy(target.Value, 0, value, 0, LLVMValueRef.CreateConstInt(_context.Handle.Int64Type, (ulong)literal.Length, false).AsValue());
    }

    public CompilerValue Literal(TypedExpression.LiteralInteger literal)
    {
        if (literal.Type.LastOutput() is not StackyType.Integer intType)
        {
            throw new InvalidCastException();
        }

        return intType switch
        {
            (false, SyntaxType.IntegerSize.S8) => new CompilerValue(LLVMValueRef.CreateConstInt(_context.Handle.Int8Type, (ulong)literal.Value, false).AsValue(), new CompilerType.Byte()),
            
            (true, SyntaxType.IntegerSize.S64) => new CompilerValue(LLVMValueRef.CreateConstInt(_context.Handle.Int64Type, (ulong)literal.Value, true).AsValue(), new CompilerType.Long()),
            
            _ => throw new NotImplementedException("TODO support other int sizes"),
        };
    }
    
    public CompilerValue LiteralByte(byte literal)
    {
        var type = _context.Handle.Int8Type;
        var value = LLVMValueRef.CreateConstInt(type, (ulong)literal, false);
        
        return new CompilerValue(value.AsValue(), new CompilerType.Byte());
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