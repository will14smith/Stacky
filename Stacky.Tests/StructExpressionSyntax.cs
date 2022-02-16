using System.Linq;
using FluentAssertions;
using Stacky.Parsing.Syntax;
using Stacky.Parsing.Typing;
using Xunit;

namespace Stacky.Tests;

public class StructExpressionSyntax : SyntaxBase
{
    [Fact]
    public void EmptyStruct_Init_ShouldParseAndInfer()
    {
        var code = @"
            struct Test { }
            test () -> Test { @Test }
        ";

        var program = ParseProgram(code);
        var typed = Infer(program);

        var function = typed.Functions.Single();
        
        var id = (TypedExpression.Identifier)function.Body;
        id.Type.Should().BeOfType<StackyType.Struct>()
            .Which.Fields.Should().BeEmpty();
    }  
    
    [Fact]
    public void EmptyStruct_InitInvalidName_ShouldParseAndInfer()
    {
        var code = @"
            struct Test { }
            test () -> Test { @Invalid }
        ";

        var program = ParseProgram(code);
        var runType = () => Infer(program);

        runType.Should().Throw<TypeInferenceException>();
    }  
    
    [Fact]
    public void Struct_GetField_ShouldParseAndInfer()
    {
        var code = @"
            struct Test { a i64 }
            test () -> i64 { @Test #a }
        ";

        var program = ParseProgram(code);
        var typed = Infer(program);

        var function = typed.Functions.Single();
        
        var getter = ((TypedExpression.Application)function.Body).Expressions[1];
        getter.Type.Should().BeOfType<StackyType.Function>()
            .Which.Output.Should().BeEquivalentTo(new StackyType.Integer(true, SyntaxType.IntegerSize.S64));
    }    
    
    [Fact]
    public void MultipleStructs_GetOverloadedField_ShouldParseAndInfer()
    {
        var code = @"
            struct Test1 { a str }
            struct Test2 { a i64 }
            test () -> i64 { @Test2 { #a } invoke }
        ";

        var program = ParseProgram(code);
        var typed = Infer(program);

        var function = typed.Functions.Single();
        
        var getter = ((TypedExpression.Application)function.Body).Expressions[1];
        getter.Type.Should().BeOfType<StackyType.Function>()
            .Which.Output.Should().BeOfType<StackyType.Function>()
            .Which.Output.Should().BeEquivalentTo(new StackyType.Integer(true, SyntaxType.IntegerSize.S64));
    }    
    
    [Fact]
    public void Struct_GetInvalidField_ShouldError()
    {
        var code = @"
            struct Test { a i64 }
            test () -> i64 { @Test #b }
        ";

        var program = ParseProgram(code);
        var runType = () => Infer(program);

        runType.Should().Throw<TypeInferenceException>();
    }    
    
    [Fact]
    public void Struct_GetInvalidType_ShouldError()
    {
        var code = @"
            struct Test { a str }
            test () -> i64 { @Test #a 1 + }
        ";

        var program = ParseProgram(code);
        var runType = () => Infer(program);

        runType.Should().Throw<TypeInferenceException>();
    }    
    
    [Fact]
    public void Struct_SetField_ShouldParseAndInfer()
    {
        var code = @"
            struct Test { a str }
            test () -> Test { @Test ""a"" ~a }
        ";

        var program = ParseProgram(code);
        var typed = Infer(program);
        
        var function = typed.Functions.Single();
        
        // TODO assert??
    }
    
    [Fact]
    public void Struct_SetInvalidField_ShouldError()
    {
        var code = @"
            struct Test { a str }
            test () -> Test { @Test 1 ~b }
        ";

        var program = ParseProgram(code);
        var runType = () => Infer(program);

        runType.Should().Throw<TypeInferenceException>();
    }  
    
    [Fact]
    public void Struct_SetInvalidType_ShouldError()
    {
        var code = @"
            struct Test { a str }
            test () -> Test { @Test 1 ~a }
        ";

        var program = ParseProgram(code);
        var runType = () => Infer(program);

        runType.Should().Throw<TypeInferenceException>();
    }    

}