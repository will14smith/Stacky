using FluentAssertions;
using Stacky.Evaluation;
using Xunit;

namespace Stacky.Tests;

public class FunctionSemantics : SemanticsBase
{
    [Fact]
    public void Functions_ShouldBeCalledWithTheirCallersStack()
    {
        var code = @"
            main () -> i64 { 2 mul3 }
            mul3 i64 -> i64 { 3 * }
        ";

        var stack = Run(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.Int64>().Which.Value.Should().Be(6);
    }
    
    [Fact]
    public void Functions_ShouldBeLeaveTheirResultsOnTheStack()
    {
        var code = @"
            main () -> i64 { const3 1 + }
            const3 () -> i64 { 3 }
        ";

        var stack = Run(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.Int64>().Which.Value.Should().Be(4);
    }
}