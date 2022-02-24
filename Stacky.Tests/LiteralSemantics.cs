using System;
using FluentAssertions;
using Stacky.Evaluation;
using Xunit;

namespace Stacky.Tests;

public class LiteralSemantics : SemanticsBase
{
    [Fact]
    public void LiteralInt_ShouldPushIntToStack()
    {
        var code = "main () -> i64 { 1 }";

        var stack = Run(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.Int64>().Which.Value.Should().Be(1);
    }
    
    [Fact]
    public void LiteralString_ShouldPushStringToStack()
    {
        var code = "main () -> str { \"a\" }";

        var stack = Run(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.String>().Which.StringValue.Should().Be("a");
    }
        
    [Fact]
    public void Addition_WithTwoNumbersOnStack_ShouldPutResultOnStack()
    {
        var code = "main () -> i64 { 1 2 + }";

        var stack = Run(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.Int64>().Which.Value.Should().Be(3);
    }    
    
    [Fact]
    public void Addition_WithTooFewArguments_ShouldError()
    {
        var code = "main () -> i64 { 1 + }";

        var run = () => Run(code);

        run.Should().Throw<Exception>();
    } 
    
    [Fact]
    public void Addition_WithStringArgument_ShouldError()
    {
        var code = "main () -> i64 { 1 \"a\" + }";

        var run = () => Run(code);

        run.Should().Throw<Exception>();
    }
}