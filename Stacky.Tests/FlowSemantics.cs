using FluentAssertions;
using Stacky.Evaluation;
using Xunit;

namespace Stacky.Tests;

public class FlowSemantics : SemanticsBase
{
    [Fact]
    public void If_WhenTrue_ShouldInvokeExpression()
    {
        var code = "main () -> i64 { 1 true { 1 + } if }";

        var stack = Run(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeEquivalentTo(new EvaluationValue.Int64(2));
    }
    
    [Fact]
    public void If_WhenFalse_ShouldSkipExpression()
    {
        var code = "main () -> i64 { 1 false { 1 + } if }";

        var stack = Run(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeEquivalentTo(new EvaluationValue.Int64(1));
    }
    
    [Fact]
    public void IfElse_WhenTrue_ShouldInvokeFirstExpression()
    {
        var code = "main () -> i64 { true { 2 } { 1 } if-else }";

        var stack = Run(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeEquivalentTo(new EvaluationValue.Int64(2));
    }
    
    [Fact]
    public void IfElse_WhenFalse_ShouldInvokeSecondExpression()
    {
        var code = "main () -> i64 { false { 2 } { 1 } if-else }";

        var stack = Run(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeEquivalentTo(new EvaluationValue.Int64(1));
    }

    [Fact]
    public void While_WhenFalse_ShouldSkipBody()
    {
        var code = "main () -> i64 { 0 { dup 0 > } { 1 - } while }";

        var stack = Run(code);

        stack.Should().HaveCount(1);
        // this is the original 0, had the loop been evaluated we'd have more than 1
        stack[0].Should().BeEquivalentTo(new EvaluationValue.Int64(0));
    }

    [Fact]
    public void While_WhileTrue_ShouldRepeatedlyCallBody()
    {
        var code = "main () -> i64 { 5 { dup 0 > } { 1 - } while }";

        var stack = Run(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeEquivalentTo(new EvaluationValue.Int64(0));
    }
}