using FluentAssertions;
using Stacky.Evaluation;
using Xunit;

namespace Stacky.Tests;

public class AnonymousFunctionSemantics : SemanticsBase
{
    [Fact]
    public void ShouldBePushedAsAValue()
    {
        var code = "{ 1 }";

        var stack = RunExpr(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.Function>();
    }

    [Fact]
    public void WithInvoke_ShouldRunLikeAFunction()
    {
        var code = "1 { 1 + } invoke";

        var stack = RunExpr(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.Int64>()
            .Which.Value.Should().Be(2);
    }
}