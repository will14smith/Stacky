using System;
using FluentAssertions;
using Stacky.Parsing;
using Stacky.Parsing.Syntax;
using Xunit;

namespace Stacky.Tests;

public class ExpressionSyntax
{
    [Fact]
    public void StringLiteral_ShouldParse()
    {
        var code = "\"str\"";

        var expr = Parse<SyntaxExpression.LiteralString>(code);

        expr.Value.Should().Be("str");
    }
    
    [Fact(Skip = "TODO not currently supported")]
    public void StringLiteral_WithEscapeSequences_ShouldParse()
    {
        var code = "\"\\\"\\n\"";

        var expr = Parse<SyntaxExpression.LiteralString>(code);

        expr.Value.Should().Be("str");
    }
    
    [Fact]
    public void NumberLiteral_ShouldParse()
    {
        var code = "123";

        var expr = Parse<SyntaxExpression.LiteralInteger>(code);

        expr.Value.Should().Be(123);
    }  
    [Fact(Skip = "TODO not currently supported")]
    public void NumberLiteral_Negative_ShouldParse()
    {
        var code = "-123";

        var expr = Parse<SyntaxExpression.LiteralInteger>(code);

        expr.Value.Should().Be(-123);
    }

    [Fact]
    public void Identifier_ShouldParse()
    {
        var code = "abc";

        var expr = Parse<SyntaxExpression.Identifier>(code);

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

        var expr = Parse<SyntaxExpression.Identifier>(code);

        expr.Value.Should().Be(identifier);
    }

    [Fact]
    public void SequenceOfExpressions_ShouldParse()
    {
        var code = "123 \"a\" hello";

        var expr = Parse<SyntaxExpression.Application>(code);

        expr.Expressions.Should().HaveCount(3);
        expr.Expressions[0].Should().BeOfType<SyntaxExpression.LiteralInteger>();
        expr.Expressions[1].Should().BeOfType<SyntaxExpression.LiteralString>();
        expr.Expressions[2].Should().BeOfType<SyntaxExpression.Identifier>();
    }
    
    [Fact]
    public void MinusThenNumber_ShouldParseAsSequence()
    {
        var code = "- 123";

        var expr = Parse<SyntaxExpression.Application>(code);

        expr.Expressions.Should().HaveCount(2);
        expr.Expressions[0].Should().BeOfType<SyntaxExpression.Identifier>();
        expr.Expressions[1].Should().BeOfType<SyntaxExpression.LiteralInteger>();
    }
    
    [Fact]
    public void UnterminatedString_ShouldError()
    {
        var code = "\"a";

        var parse = () => Parse(code);

        parse.Should().Throw<Exception>();
    }
    
    private static T Parse<T>(string exprCode) where T : SyntaxExpression =>
        Parse(exprCode).Should().BeOfType<T>().Subject;

    private static SyntaxExpression Parse(string exprCode)
    {
        var code = $"run () -> () {{ {exprCode} }}";
        
        var parser = new Parser("test", code);
        var program = parser.Parse();

        program.Functions.Should().HaveCount(1);
        return program.Functions[0].Body;
    }
}