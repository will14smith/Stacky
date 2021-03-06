using System;
using FluentAssertions;
using Stacky.Evaluation;
using Xunit;

namespace Stacky.Tests;

public class BindingSemantics : SemanticsBase
{
    [Fact]
    public void BindingOneVariable_ShouldRemoveFromStack()
    {
        var code = "main () -> () { 1 (a) { } }";

        var stack = Run(code);

        stack.Should().BeEmpty();
    }

    [Fact]
    public void BindingOneVariable_ReferencingTheVariable_ShouldPushVariableBackToStack()
    {
        var code = "main () -> i64 { 1 (a) { a } }";

        var stack = Run(code);

        stack.Should().HaveCount(1);
    }   
    
    [Fact]
    public void BindingOneVariable_ReferencingTheVariableInAFunction_ShouldCaptureInClosure()
    {
        var code = "main () -> i64 { 1 (a) { { a } } invoke }";

        var stack = Run(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.Int64>()
            .Which.Value.Should().Be(1);
    }   
    
    [Fact]
    public void BindingOneVariable_ReferencingTheVariableInAFunction_ShouldCaptureTheCorrectBinding()
    {
        var code = "main () -> i64 { 1 (a) { { a } } 2 (a) { invoke } }";

        var stack = Run(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.Int64>()
            .Which.Value.Should().Be(1);
    }   
    
    [Fact]
    public void BindingOneVariable_ReferencingTheVariableMultipleTimes_ShouldPushVariableBackToStackMultipleTimes()
    {
        var code = "main () -> i64 i64 { 1 (a) { a a } }";

        var stack = Run(code);

        stack.Should().HaveCount(2);
    }

    [Fact]
    public void BindingMultipleVariable_ShouldPopAllValues()
    {
        var code = "main () -> () { 1 2 3 (a b c) { } }";

        var stack = Run(code);

        stack.Should().BeEmpty();
    }
    
    [Fact]
    public void BindingMultipleVariable_ShouldReferenceInStackOrder()
    {
        var code = "main () -> i64 i64 i64 { 1 2 3 (a b c) { a b c } }";

        var stack = Run(code);

        stack.Should().HaveCount(3);
        stack[0].Should().BeOfType<EvaluationValue.Int64>().Which.Value.Should().Be(3);
        stack[1].Should().BeOfType<EvaluationValue.Int64>().Which.Value.Should().Be(2);
        stack[2].Should().BeOfType<EvaluationValue.Int64>().Which.Value.Should().Be(1);
    }
    
    [Fact]
    public void BindingMultipleVariableWithOverloads_ShouldUseLastBindingAsValue()
    {
        var code = "main () -> i64 { 1 2 (_ _) { _ } }";

        var stack = Run(code);

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.Int64>().Which.Value.Should().Be(2);
    }
    
    
}