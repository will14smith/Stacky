using Stacky.Language.Syntax;

namespace Stacky.Language.Compilation;

public class CompilerTypeBuilder
{
    public CompilerType.Function Build(SyntaxFunction function)
    {
        var inputs = function.Input.Select(Build).ToList();
        var outputs = function.Output.Select(Build).ToList();
        
        return new CompilerType.Function(inputs, outputs);
    }
    
    public CompilerType Build(SyntaxTypeReference typeRef)
    {
        return typeRef.Type switch
        {
            SyntaxType.Integer(var signed, var size) => size switch
            {
                SyntaxType.IntegerSize.S64 => signed ? new CompilerType.Long() : throw new NotImplementedException(),
                
                _ => throw new NotImplementedException()
            },

            SyntaxType.String => new CompilerType.String(),
            
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}