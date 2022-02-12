using System;
using System.Collections.Generic;
using FluentAssertions;
using Stacky.Parsing;
using Stacky.Parsing.Syntax;
using Xunit;

namespace Stacky.Tests;

public class FunctionSyntax
{
    [Fact]
    public void NoInputsOrOutputs_ShouldParse()
    {
        var code = "run () -> () { }";

        var function = Parse(code);

        function.Name.Value.Should().Be("run");
        function.Input.Should().BeEmpty();
        function.Output.Should().BeEmpty();
    }
    
    [Fact]
    public void SingleInput_ShouldParse()
    {
        var code = "run i8 -> () { }";
        
        var function = Parse(code);

        function.Input.Should().BeEquivalentTo(new [] { new SyntaxType.Integer(SyntaxPosition.Empty, true, SyntaxType.IntegerSize.S8) }, x => x.Excluding(t => t.Position));
        function.Output.Should().BeEmpty();
    } 
    
    [Fact]
    public void SingleOutput_ShouldParse()
    {
        var code = "run () -> i8 { }";
        
        var function = Parse(code);

        function.Input.Should().BeEmpty();
        function.Output.Should().BeEquivalentTo(new [] { new SyntaxType.Integer(SyntaxPosition.Empty, true, SyntaxType.IntegerSize.S8) }, x => x.Excluding(t => t.Position));
    }
    
    [Fact]
    public void MultipleInputsAndOutputs_ShouldParse()
    {
        var code = "run i8 i16 -> i32 i64 { }";

        var function = Parse(code);

        function.Input.Should().BeEquivalentTo(new []
        {
            new SyntaxType.Integer(SyntaxPosition.Empty, true, SyntaxType.IntegerSize.S8), 
            new SyntaxType.Integer(SyntaxPosition.Empty, true, SyntaxType.IntegerSize.S16)
        }, x => x.WithStrictOrdering().Excluding(t => t.Position));
        function.Output.Should().BeEquivalentTo(new []
        {
            new SyntaxType.Integer(SyntaxPosition.Empty, true, SyntaxType.IntegerSize.S32),
            new SyntaxType.Integer(SyntaxPosition.Empty, true, SyntaxType.IntegerSize.S64)
        }, x => x.WithStrictOrdering().Excluding(t => t.Position));
    }

    [Fact]
    public void ShouldParseBody()
    {
        var code = "run () -> () { 1 drop }";

        var function = Parse(code);

        function.Body.Should().NotBeNull();
    }
    
    public static IEnumerable<object[]> PrimitiveTypes
    {
        get
        {
            yield return new object[] { "bool", new SyntaxType.Boolean(SyntaxPosition.Empty) };
            yield return new object[] { "u8", new SyntaxType.Integer(SyntaxPosition.Empty, false, SyntaxType.IntegerSize.S8) };
            yield return new object[] { "i8", new SyntaxType.Integer(SyntaxPosition.Empty, true, SyntaxType.IntegerSize.S8) };
            yield return new object[] { "u16", new SyntaxType.Integer(SyntaxPosition.Empty, false, SyntaxType.IntegerSize.S16) };
            yield return new object[] { "i16", new SyntaxType.Integer(SyntaxPosition.Empty, true, SyntaxType.IntegerSize.S16) };
            yield return new object[] { "u32", new SyntaxType.Integer(SyntaxPosition.Empty, false, SyntaxType.IntegerSize.S32) };
            yield return new object[] { "i32", new SyntaxType.Integer(SyntaxPosition.Empty, true, SyntaxType.IntegerSize.S32) };
            yield return new object[] { "u64", new SyntaxType.Integer(SyntaxPosition.Empty, false, SyntaxType.IntegerSize.S64) };
            yield return new object[] { "i64", new SyntaxType.Integer(SyntaxPosition.Empty, true, SyntaxType.IntegerSize.S64) };
            yield return new object[] { "str", new SyntaxType.String(SyntaxPosition.Empty) };
        }
    }
    
    [Theory]
    [MemberData(nameof(PrimitiveTypes))]
    public void PrimitiveType_ShouldParse(string name, SyntaxType expected)
    {
        var code = $"run {name} -> () {{ }}";
        
        var function = Parse(code);

        function.Input.Should().ContainSingle()
            .Which.Should().BeOfType(expected.GetType());

        if (expected is SyntaxType.Integer)
        {
            function.Input[0].Should().BeEquivalentTo(expected, x => x.IncludingAllRuntimeProperties().Excluding(t => t.Position));
        }
        
        function.Output.Should().BeEmpty();
    }
    
    [Fact]
    public void MissingTypes_ShouldError()
    {
        var code = "run { }";

        var parse = () => Parse(code);

        parse.Should().Throw<Exception>();
    }   
    
    [Fact]
    public void MissingInputTypes_ShouldError()
    {
        var code = "run -> () { }";

        var parse = () => Parse(code);

        parse.Should().Throw<Exception>();
    }   
    
    [Fact]
    public void MissingOutputTypes_ShouldError()
    {
        var code = "run () -> { }";

        var parse = () => Parse(code);

        parse.Should().Throw<Exception>();
    }

    private static SyntaxFunction Parse(string code)
    {
        var parser = new Parser("test", code);
        var program = parser.Parse();

        program.Functions.Should().HaveCount(1);
        return program.Functions[0];
    }
}