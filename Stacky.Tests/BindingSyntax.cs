using FluentAssertions;
using Stacky.Parsing.Syntax;
using Xunit;

namespace Stacky.Tests;

public class BindingSyntax : SyntaxBase
{
    [Fact]
    public void BindingOneVariable()
    {
        var code = "(a) { }";

        var expr = ParseExpr<SyntaxExpression.Binding>(code);

        expr.Names.Should().HaveCount(1);
        expr.Names[0].Value.Should().Be("a");
    }

    [Fact]
    public void BindingOneVariable_ReferencingTheVariable()
    {
        var code = "(a) { a }";

        var expr = ParseExpr<SyntaxExpression.Binding>(code);

        expr.Body.Should().BeOfType<SyntaxExpression.Identifier>()
            .Which.Value.Should().Be("a");
    }

    [Fact]
    public void BindingMultipleVariable()
    {
        var code = "(a b c) { }";

        var expr = ParseExpr<SyntaxExpression.Binding>(code);

        expr.Names.Should().HaveCount(3);
        expr.Names[0].Value.Should().Be("a");
        expr.Names[1].Value.Should().Be("b");
        expr.Names[2].Value.Should().Be("c");
    }
    
    [Fact]
    public void BindingMultipleVariableWithOverloads()
    {
        var code = "(a _ _) { }";

        var expr = ParseExpr<SyntaxExpression.Binding>(code);

        expr.Names.Should().HaveCount(3);
        expr.Names[0].Value.Should().Be("a");
        expr.Names[1].Value.Should().Be("_");
        expr.Names[2].Value.Should().Be("_");
    }
}