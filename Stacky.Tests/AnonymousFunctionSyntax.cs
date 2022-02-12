using FluentAssertions;
using Stacky.Parsing.Syntax;
using Xunit;

namespace Stacky.Tests;

public class AnonymousFunctionSyntax : SyntaxBase
{
    [Fact]
    public void Empty_ShouldParse()
    {
        var code = "{ }";
        
        var expr = ParseExpr<SyntaxExpression.Function>(code);

        expr.Body.Should().BeOfType<SyntaxExpression.Application>()
            .Which.Expressions.Should().BeEmpty();
    }  
    
    [Fact]
    public void WithBody_ShouldParseWithBody()
    {
        var code = "{ 1 }";
        
        var expr = ParseExpr<SyntaxExpression.Function>(code);

        expr.Body.Should().BeOfType<SyntaxExpression.LiteralInteger>()
            .Which.Value.Should().Be(1);
    } 
    
    [Fact]
    public void WithApplicationBody_ShouldParseWithBody()
    {
        var code = "{ 1 + }";
        
        var expr = ParseExpr<SyntaxExpression.Function>(code);

        expr.Body.Should().BeOfType<SyntaxExpression.Application>()
            .Which.Expressions.Should().HaveCount(2);
    } 

    [Fact]
    public void MultipleInApplication_ShouldParse()
    {
        var code = "{ 1 } { \"a\" }";
        
        var expr = ParseExpr<SyntaxExpression.Application>(code);

        expr.Expressions.Should().HaveCount(2).And.AllBeOfType<SyntaxExpression.Function>();
    }
}