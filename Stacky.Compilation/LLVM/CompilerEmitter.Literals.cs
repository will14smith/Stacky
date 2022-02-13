using LLVMSharp;
using Stacky.Parsing.Syntax;
using Stacky.Parsing.Typing;
using L = LLVMSharp.LLVM;

namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    public CompilerValue Literal(string literal)
    {
        var value = L.BuildGlobalStringPtr(_builder, literal, "str");
        // TODO this should be copied from global -> gc heap
        return new CompilerValue(value, new CompilerType.String());
    }

    public CompilerValue Literal(TypedExpression.LiteralInteger literal)
    {
        if (literal.Type is not StackyType.Integer intType)
        {
            throw new InvalidCastException();
        }

        if (intType.Signed != true || intType.Size != SyntaxType.IntegerSize.S64)
        {
            throw new NotImplementedException("TODO support other int sizes");
        }
        
        var value = L.ConstInt(LLVMTypeRef.Int64TypeInContext(_context), (ulong)literal.Value, true);
        return new CompilerValue(value, new CompilerType.Long());
    }
    public CompilerValue Literal(long literal)
    {
        var value = L.ConstInt(LLVMTypeRef.Int64TypeInContext(_context), (ulong)literal, true);
        return new CompilerValue(value, new CompilerType.Long());
    }
    
    public CompilerValue Literal(bool literal)
    {
        var value = L.ConstInt(LLVMTypeRef.Int1Type(), literal ? 1u : 0u, false);
        return new CompilerValue(value, new CompilerType.Boolean());
    }
}