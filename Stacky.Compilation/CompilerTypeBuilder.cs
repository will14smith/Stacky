using Stacky.Parsing.Syntax;
using Stacky.Parsing.Typing;

namespace Stacky.Compilation;

public class CompilerTypeBuilder
{
    public CompilerType.Function BuildFunction(StackyType.Function function)
    {
        var inputs = StackyType.Iterator(function.Input).Select(Build).ToList();
        var outputs = StackyType.Iterator(function.Output).Select(Build).ToList();
        
        return new CompilerType.Function(inputs, outputs);
    }
    
    public CompilerType Build(StackyType type)
    {
        return type switch
        {
            StackyType.Boolean => new CompilerType.Boolean(),
            
            StackyType.Integer(var signed, var size) => size switch
            {
                SyntaxType.IntegerSize.S64 => signed ? new CompilerType.Long() : throw new NotImplementedException(),
                
                _ => throw new NotImplementedException()
            },

            StackyType.String => new CompilerType.String(),
            
            StackyType.Function function => BuildFunction(function),
            
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}