using Stacky.Parsing.Syntax;

namespace Stacky.Compilation;

public class CompilerTypeBuilder
{
    public CompilerType.Function Build(SyntaxFunction function)
    {
        var inputs = function.Input.Select(Build).ToList();
        var outputs = function.Output.Select(Build).ToList();
        
        return new CompilerType.Function(inputs, outputs);
    }
    
    public CompilerType Build(SyntaxType type)
    {
        return type switch
        {
            SyntaxType.Integer(_, var signed, var size) => size switch
            {
                SyntaxType.IntegerSize.S64 => signed ? new CompilerType.Long() : throw new NotImplementedException(),
                
                _ => throw new NotImplementedException()
            },

            SyntaxType.String => new CompilerType.String(),
            
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}