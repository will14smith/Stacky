using FluentAssertions;
using Stacky.Evaluation;
using Xunit;

namespace Stacky.Tests;

public class FlowSemantics : SemanticsBase
{
    [Fact]
    public void If_WhenTrue_ShouldInvokeExpression()
    {
        var code = "1 true { 1 + } if";

        var stack = RunExpr(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeEquivalentTo(new EvaluationValue.Int64(2));
    }
    
    [Fact]
    public void If_WhenFalse_ShouldSkipExpression()
    {
        var code = "1 false { 1 + } if";

        var stack = RunExpr(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeEquivalentTo(new EvaluationValue.Int64(1));
    }
    
    [Fact]
    public void IfElse_WhenTrue_ShouldInvokeFirstExpression()
    {
        var code = "true { 2 } { 1 } if-else";

        var stack = RunExpr(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeEquivalentTo(new EvaluationValue.Int64(2));
    }
    
    [Fact]
    public void IfElse_WhenFalse_ShouldInvokeSecondExpression()
    {
        var code = "false { 2 } { 1 } if-else";

        var stack = RunExpr(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeEquivalentTo(new EvaluationValue.Int64(1));
    }

    [Fact]
    public void While_WhenFalse_ShouldSkipBody()
    {
        // note: this is a _bad_ loop since it grows the stack each iteration
        var code = "0 { dup 0 > } { dup 1 - } while";

        var stack = RunExpr(code);

        stack.Should().HaveCount(1);
        // this is the original 0, had the loop been evaluated we'd have more than 1
        stack[0].Should().BeEquivalentTo(new EvaluationValue.Int64(0));
    }

    [Fact]
    public void While_WhileTrue_ShouldRepeatedlyCallBody()
    {
        // note: this is a _bad_ loop since it grows the stack each iteration
        var code = "5 { dup 0 > } { dup 1 - } while";

        var stack = RunExpr(code);

        stack.Should().HaveCount(6);
        stack[0].Should().BeEquivalentTo(new EvaluationValue.Int64(0));
        stack[1].Should().BeEquivalentTo(new EvaluationValue.Int64(1));
        stack[2].Should().BeEquivalentTo(new EvaluationValue.Int64(2));
        stack[3].Should().BeEquivalentTo(new EvaluationValue.Int64(3));
        stack[4].Should().BeEquivalentTo(new EvaluationValue.Int64(4));
        stack[5].Should().BeEquivalentTo(new EvaluationValue.Int64(5));
    }
}