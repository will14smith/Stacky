using System;
using FluentAssertions;
using Stacky.Parsing.Syntax;
using Xunit;

namespace Stacky.Tests;

public class ExpressionSyntax : SyntaxBase
{
    [Fact]
    public void StringLiteral_ShouldParse()
    {
        var code = "\"str\"";

        var expr = ParseExpr<SyntaxExpression.LiteralString>(code);

        expr.Value.Should().Be("str");
    }
    
    [Fact(Skip = "TODO not currently supported")]
    public void StringLiteral_WithEscapeSequences_ShouldParse()
    {
        var code = "\"\\\"\\n\"";

        var expr = ParseExpr<SyntaxExpression.LiteralString>(code);

        expr.Value.Should().Be("str");
    }
    
    [Fact]
    public void NumberLiteral_ShouldParse()
    {
        var code = "123";

        var expr = ParseExpr<SyntaxExpression.LiteralInteger>(code);

        expr.Value.Should().Be(123);
    }  
    [Fact(Skip = "TODO not currently supported")]
    public void NumberLiteral_Negative_ShouldParse()
    {
        var code = "-123";

        var expr = ParseExpr<SyntaxExpression.LiteralInteger>(code);

        expr.Value.Should().Be(-123);
    }

    [Fact]
    public void Identifier_ShouldParse()
    {
        var code = "abc";

        var expr = ParseExpr<SyntaxExpression.Identifier>(code);

        expr.Value.Should().Be("abc");
    }
    
    [Theory]
    [InlineData("+")]
    [InlineData("++")]
    [InlineData(">")]
    [InlineData("!")]
    [InlineData("@")]
    [InlineData("-")]
    public void Identifier_WithSpecialChars_ShouldParse(string identifier)
    {
        var code = identifier;

        var expr = ParseExpr<SyntaxExpression.Identifier>(code);

        expr.Value.Should().Be(identifier);
    }

    [Fact]
    public void SequenceOfExpressions_ShouldParse()
    {
        var code = "123 \"a\" hello";

        var expr = ParseExpr<SyntaxExpression.Application>(code);

        expr.Expressions.Should().HaveCount(3);
        expr.Expressions[0].Should().BeOfType<SyntaxExpression.LiteralInteger>();
        expr.Expressions[1].Should().BeOfType<SyntaxExpression.LiteralString>();
        expr.Expressions[2].Should().BeOfType<SyntaxExpression.Identifier>();
    }
    
    [Fact]
    public void MinusThenNumber_ShouldParseAsSequence()
    {
        var code = "- 123";

        var expr = ParseExpr<SyntaxExpression.Application>(code);

        expr.Expressions.Should().HaveCount(2);
        expr.Expressions[0].Should().BeOfType<SyntaxExpression.Identifier>();
        expr.Expressions[1].Should().BeOfType<SyntaxExpression.LiteralInteger>();
    }
    
    [Fact]
    public void UnterminatedString_ShouldError()
    {
        var code = "\"a";

        var parse = () => ParseExpr(code);

        parse.Should().Throw<Exception>();
    }
}