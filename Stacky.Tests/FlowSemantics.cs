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
}