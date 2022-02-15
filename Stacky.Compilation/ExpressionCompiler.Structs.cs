using Stacky.Parsing.Typing;

namespace Stacky.Compilation;

public partial class ExpressionCompiler
{
    private CompilerStack CompileInit(CompilerStack stack, TypedExpression.Identifier identifier)
    {
        var structName = identifier.Value[1..];
        var structType = _environment.GetStruct(structName);

        var target = _allocator.Allocate(structType);
        target = _emitter.StructInit(target);

        return stack.Push(target);
    }

    private CompilerStack CompileGetter(CompilerStack stack, TypedExpression.Identifier identifier)
    {
        var fieldName = identifier.Value[1..];

        throw new NotImplementedException();
    }

    private CompilerStack CompileSetter(CompilerStack stack, TypedExpression.Identifier identifier)
    {
        var fieldName = identifier.Value[1..];

        throw new NotImplementedException();
    }
}