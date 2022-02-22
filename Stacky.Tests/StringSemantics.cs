using FluentAssertions;
using Stacky.Evaluation;
using Xunit;

namespace Stacky.Tests;

public class StringSemantics : SemanticsBase
{
    [Fact]
    public void StringIndexGetter_ShouldReturnCharAtIndex()
    {
        var stack = RunExpr("\"abc\" 1 !");

        stack.Should().HaveCount(1);
        // TODO should be u8
        stack[0].Should().BeOfType<EvaluationValue.Int64>().Which.Value.Should().Be((byte)'b');
    }
    
    [Fact]
    public void StringIndexSetter_ShouldSetCharAtIndex()
    {
        var stack = RunExpr($"\"abc\" {(byte)'X'} 1 !~");

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.String>().Which.StringValue.Should().Be("aXc");
    }    
    
    [Fact]
    public void StringRangeGetter_ShouldReturnCharsBetweenIndexes()
    {
        var stack = RunExpr("\"abcdef\" 1 4 !!");

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.String>().Which.StringValue.Should().Be("bcd");
    }   
    
    [Fact]
    public void StringRangeSetter_ShouldSetCharsStartingAtIndex()
    {
        var stack = RunExpr("\"abcdef\" \"XXX\" 1 !!~");

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.String>().Which.StringValue.Should().Be("aXXXef");
    }    
    
    [Fact]
    public void StringInit_ShouldMakeNewEmptyStringOfLength()
    {
        var stack = RunExpr("5 !@");

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.String>()
            // this value is "undefined" until the user sets the chars
            .Which.StringValue.Should().HaveLength(5).And.Be("     ");
    }  
    
    [Fact]
    public void StringConcat_ShouldJoinStrings()
    {
        var stack = RunExpr("\"abc\" \"def\" concat");

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.String>().Which.StringValue.Should().Be("abcdef");
    }  
    
    [Fact]
    public void StringLength_ShouldReturnLengthOfString()
    {
        var stack = RunExpr("\"abc\" length");

        stack.Should().HaveCount(1);
        stack[0].Should().BeOfType<EvaluationValue.Int64>().Which.Value.Should().Be(3);
    }
}