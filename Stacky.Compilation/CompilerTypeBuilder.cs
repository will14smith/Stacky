using Stacky.Compilation.LLVM;
using Stacky.Parsing.Syntax;
using Stacky.Parsing.Typing;

namespace Stacky.Compilation;

public class CompilerTypeBuilder
{
    public CompilerType.Function BuildFunction(StackyType.Function function)
    {
        var inputs = StackyType.Iterator(function.Input).Where(x => x is not StackyType.Void).Select(Build).ToList();
        var outputs = StackyType.Iterator(function.Output).Where(x => x is not StackyType.Void).Select(Build).ToList();

        return new CompilerType.Function(inputs, outputs);
    }

    public CompilerType.Struct BuildStruct(StackyType.Struct type)
    {
        var fields = type.Fields.Select(Build).ToList();
        
        return new CompilerType.Struct(type.Name, fields);
    }

    private CompilerType.StructField Build(StackyType.StructField field) => new(field.Name, Build(field.Type));

    public CompilerType Build(StackyType type)
    {
        return type switch
        {
            StackyType.Boolean => new CompilerType.Boolean(),
            
            StackyType.Integer(var signed, var size) => size switch
            {
                SyntaxType.IntegerSize.S8 => signed ? throw new NotImplementedException() : new CompilerType.Byte(),
                SyntaxType.IntegerSize.S64 => signed ? new CompilerType.Long() : throw new NotImplementedException(),
                
                _ => throw new NotImplementedException()
            },

            StackyType.String => new CompilerType.String(),
            
            StackyType.Function function => BuildFunction(function),
            StackyType.Struct definition => new CompilerType.Pointer(BuildStruct(definition)),
            
            ICompilerTypeConversion conversion => conversion.ToCompilerType(),
            
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}