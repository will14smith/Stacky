using FluentAssertions;
using Stacky.Parsing;
using Stacky.Parsing.Syntax;

namespace Stacky.Tests;

public class SyntaxBase
{
    protected static T ParseExpr<T>(string exprCode) where T : SyntaxExpression =>
        ParseExpr(exprCode).Should().BeOfType<T>().Subject;

    protected static SyntaxExpression ParseExpr(string exprCode)
    {
        var code = $"run () -> () {{ {exprCode} }}";
        
        var function = Parse(code);
        return function.Body;
    }
    
    protected static SyntaxFunction Parse(string code)
    {
        var program = ParseProgram(code);

        program.Functions.Should().HaveCount(1);
        return program.Functions[0];
    }
    
    protected static SyntaxProgram ParseProgram(string code)
    {
        var parser = new Parser("test", code);
        return parser.Parse();
    }

}