using FluentAssertions;
using Stacky.Evaluation;
using Xunit;

namespace Stacky.Tests;

public class AnonymousFunctionSemantics : SemanticsBase
{
    [Fact]
    public void ShouldBePushedAsAValue()
    {
        var code = "main () -> (() -> i64) { { 1 } }";

        var stack = Run(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.Closure>();
    }

    [Fact]
    public void WithInvoke_ShouldRunLikeAFunction()
    {
        var code = "main () -> i64 { 1 { 1 + } invoke }";

        var stack = Run(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.Int64>()
            .Which.Value.Should().Be(2);
    }
}