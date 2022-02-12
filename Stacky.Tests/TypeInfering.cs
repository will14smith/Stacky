using System;
using System.Linq;
using FluentAssertions;
using Stacky.Parsing.Syntax;
using Stacky.Parsing.Typing;
using Xunit;

namespace Stacky.Tests;

public class TypeInfering : SyntaxBase
{
    [Fact]
    public void NumberLiteral_ShouldBeNumeric()
    {
        var program = ParseProgram("test () -> i64 { 1 }");

        var typed = TypeInferer.Infer(program);

        var function = typed.Functions.Single();
        var expr = function.Body;
        expr.Type.Should().BeEquivalentTo(new StackyType.Integer(true, SyntaxType.IntegerSize.S64));
    }

    [Fact]
    public void StringLiteral_ShouldBeNumeric()
    {
        var program = ParseProgram("test () -> str { \"a\" }");

        var typed = TypeInferer.Infer(program);

        var function = typed.Functions.Single();
        var expr = function.Body;
        expr.Type.Should().BeOfType<StackyType.String>();
    }
    
    [Fact]
    public void MultipleNumberLiteral_ShouldBeMultipleNumeric()
    {
        var program = ParseProgram("test () -> i64 i64 { 1 2 }");

        var typed = TypeInferer.Infer(program);

        var function = typed.Functions.Single();
        var expr = function.Body;
        expr.Type.Should().BeEquivalentTo(new StackyType.Function(Array.Empty<StackyType>(), new []
        {
            new StackyType.Integer(true, SyntaxType.IntegerSize.S64),
            new StackyType.Integer(true, SyntaxType.IntegerSize.S64)
        }));
    }

    [Fact]
    public void Application_WithAllArgsProvided_ShouldBeEmpty()
    {
        var program = ParseProgram("test () -> () { \"a\" print }");

        var typed = TypeInferer.Infer(program);

        var function = typed.Functions.Single();
        var expr = function.Body;
        expr.Type.Should().BeEquivalentTo(new StackyType.Function(Array.Empty<StackyType>(), Array.Empty<StackyType>()));
    }  
    
    [Fact]
    public void Application_WithSomeArgsProvided_ShouldHaveInput()
    {
        var program = ParseProgram("test i64 -> () { 1 + print }");

        var typed = TypeInferer.Infer(program);

        var function = typed.Functions.Single();
        var expr = function.Body;
        expr.Type.Should().BeEquivalentTo(new StackyType.Function(new []
        {
            new StackyType.Integer(true, SyntaxType.IntegerSize.S64),
        }, Array.Empty<StackyType>()));
    }
    
    [Fact]
    public void Application_WithMultipleArgsNotProvided_ShouldHaveMultipleInput()
    {
        var program = ParseProgram("test str i64 -> str { 1 + string concat }");

        var typed = TypeInferer.Infer(program);

        var function = typed.Functions.Single();
        var expr = function.Body;
        expr.Type.Should().BeEquivalentTo(new StackyType.Function(new StackyType[]
        {
            new StackyType.String(),
            new StackyType.Integer(true, SyntaxType.IntegerSize.S64),
        }, new []
        {
            new StackyType.String(),
        }));
    }
    
    [Fact]
    public void Application_UserDefinedFunction_ShouldInfer()
    {
        var program = ParseProgram(@"
            test1 i64 -> str { test2 + + string }
            test2 () -> i64 i64 { 1 2 }
        ");

        var typed = TypeInferer.Infer(program);

        var function = typed.Functions.First();
        var expr = function.Body;
        expr.Type.Should().BeEquivalentTo(new StackyType.Function(new StackyType[]
        {
            new StackyType.Integer(true, SyntaxType.IntegerSize.S64),
        }, new []
        {
            new StackyType.String(),
        }));
    }
   
    // TODO anonymous functions (:
}