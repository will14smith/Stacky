using FluentAssertions;
using Stacky.Parsing.Syntax;
using Xunit;

namespace Stacky.Tests;

public class StructSyntax : SyntaxBase
{
    [Fact]
    public void EmptyStruct_CanBeParsed()
    {
        var code = @"struct Test { }";

        var program = ParseProgram(code);

        program.Structs.Should().HaveCount(1);
        
        var type = program.Structs[0];
        type.Name.Value.Should().Be("Test");
        type.Fields.Should().BeEmpty();
    }

    [Fact]
    public void StructWithOneField_CanBeParsed()
    {
        var code = @"struct Test { a i64 }";

        var program = ParseProgram(code);

        program.Structs.Should().HaveCount(1);
        
        var type = program.Structs[0];
        type.Name.Value.Should().Be("Test");
        type.Fields.Should().HaveCount(1);
        
        var field = type.Fields[0];
        field.Name.Value.Should().Be("a");
        field.Type.Should().BeEquivalentTo(new SyntaxType.Integer(SyntaxPosition.Empty, true, SyntaxType.IntegerSize.S64), o => o.Excluding(s => s.Position));
    }

    [Fact]
    public void StructWithMultipleFields_CanBeParsed()
    {
        var code = @"struct Test { a i64 b str }";

        var program = ParseProgram(code);

        program.Structs.Should().HaveCount(1);
        
        var type = program.Structs[0];
        type.Name.Value.Should().Be("Test");
        type.Fields.Should().HaveCount(2);
        
        var field1 = type.Fields[0];
        field1.Name.Value.Should().Be("a");
        field1.Type.Should().BeEquivalentTo(new SyntaxType.Integer(SyntaxPosition.Empty, true, SyntaxType.IntegerSize.S64), o => o.Excluding(s => s.Position));
     
        var field2 = type.Fields[1];
        field2.Name.Value.Should().Be("b");
        field2.Type.Should().BeOfType<SyntaxType.String>();
    }

    [Fact]
    public void StructWithOtherStructField_CanBeParsed()
    {
        var code = @"
            struct Test1 { a Test2 }
            struct Test2 { a i64 }
        ";

        var program = ParseProgram(code);

        program.Structs.Should().HaveCount(2);
        
        var type = program.Structs[0];
        type.Name.Value.Should().Be("Test1");
        type.Fields.Should().HaveCount(1);
        
        var field = type.Fields[0];
        field.Name.Value.Should().Be("a");
        field.Type.Should().BeEquivalentTo(new SyntaxType.Struct(SyntaxPosition.Empty, "Test2"), o => o.Excluding(s => s.Position));
    }

    [Fact]
    public void StructWithSameStructField_CanBeParsed()
    {
        var code = @"struct Test1 { a Test1 }";

        var program = ParseProgram(code);

        program.Structs.Should().HaveCount(1);
        
        var type = program.Structs[0];
        type.Name.Value.Should().Be("Test1");
        type.Fields.Should().HaveCount(1);
        
        var field = type.Fields[0];
        field.Name.Value.Should().Be("a");
        field.Type.Should().BeEquivalentTo(new SyntaxType.Struct(SyntaxPosition.Empty, "Test1"), o => o.Excluding(s => s.Position));

    }
}