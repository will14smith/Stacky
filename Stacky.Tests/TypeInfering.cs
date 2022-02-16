using System;
using System.Linq;
using FluentAssertions;
using Stacky.Intrinsics;
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

        var typed = Infer(program);

        var function = typed.Functions.Single();
        var expr = function.Body;
        expr.Type.Should().BeEquivalentTo(new StackyType.Integer(true, SyntaxType.IntegerSize.S64));
    }
    
    [Fact]
    public void StringLiteral_ShouldBeNumeric()
    {
        var program = ParseProgram("test () -> str { \"a\" }");

        var typed = Infer(program);

        var function = typed.Functions.Single();
        var expr = function.Body;
        expr.Type.Should().BeOfType<StackyType.String>();
    }
    
    [Fact]
    public void MultipleNumberLiteral_ShouldBeMultipleNumeric()
    {
        var program = ParseProgram("test () -> i64 i64 { 1 2 }");

        var typed = Infer(program);

        var function = typed.Functions.Single();
        var expr = function.Body;
        expr.Type.Should().BeEquivalentTo(new StackyType.Function(new StackyType.Void(), new StackyType.Composite(new []
        {
            new StackyType.Integer(true, SyntaxType.IntegerSize.S64),
            new StackyType.Integer(true, SyntaxType.IntegerSize.S64)
        })));
    }
    
    [Fact]
    public void StringReturnedFromNumber_ShouldError()
    {
        var program = ParseProgram("test () -> i64 { \"a\" }");

        var func = () => Infer(program);

        func.Should().Throw<TypeInferenceException>();
    }

    [Fact]
    public void StringPassedToAddition_ShouldError()
    {
        var program = ParseProgram("test () -> i64 { \"a\" 1 + }");

        var func = () => Infer(program);

        func.Should().Throw<TypeInferenceException>();
    }
    
    [Fact]
    public void NumberLiteralWithNoHints_ShouldPickDefaultType()
    {
        var program = ParseProgram("test () -> () { 1 print }");

        var typed = Infer(program);

        var function = typed.Functions.Single();
        
        var literal = ((TypedExpression.Application)function.Body).Expressions[0];
        literal.Type.Should().BeEquivalentTo(new StackyType.Integer(true, SyntaxType.IntegerSize.S64));
    }

    
    [Fact]
    public void Application_WithAllArgsProvided_ShouldBeEmpty()
    {
        var program = ParseProgram("test () -> () { \"a\" print }");

        var typed = Infer(program);

        var function = typed.Functions.Single();
        var expr = function.Body;
        expr.Type.Should().BeEquivalentTo(new StackyType.Function(new StackyType.Void(), new StackyType.Void()));
    }  
    
    [Fact]
    public void Application_WithSomeArgsProvided_ShouldHaveInput()
    {
        var program = ParseProgram("test i64 -> () { 1 + print }");

        var typed = Infer(program);

        var function = typed.Functions.Single();
        var expr = function.Body;
        expr.Type.Should().BeEquivalentTo(new StackyType.Function(
            new StackyType.Integer(true, SyntaxType.IntegerSize.S64),
            new StackyType.Void()));
    }
    
    [Fact]
    public void Application_WithMultipleArgsNotProvided_ShouldHaveMultipleInput()
    {
        var program = ParseProgram("test str i64 -> str { 1 + string concat }");

        var typed = Infer(program);

        var function = typed.Functions.Single();
        var expr = function.Body;
        expr.Type.Should().BeEquivalentTo(new StackyType.Function(
            new StackyType.Composite(new StackyType[]
            {
                new StackyType.String(),
                new StackyType.Integer(true, SyntaxType.IntegerSize.S64),
            }), 
            new StackyType.String()
        ));
    }
    
    [Fact]
    public void Application_UserDefinedFunction_ShouldInfer()
    {
        var program = ParseProgram(@"
            test1 i64 -> str { test2 + + string }
            test2 () -> i64 i64 { 1 2 }
        ");

        var typed = Infer(program);

        var function = typed.Functions.First();
        var expr = function.Body;
        expr.Type.Should().BeEquivalentTo(new StackyType.Function(
            new StackyType.Integer(true, SyntaxType.IntegerSize.S64),
            new StackyType.String()
        ));
    }
   
    
    [Fact]
    public void AnonymousFunction_WithInputs_ShouldInfer()
    {
        var program = ParseProgram(@"test i64 -> () { { 1 + print } invoke }");

        var typed = Infer(program);

        var function = typed.Functions.First();
        var expr = function.Body;
        expr.Type.Should().BeEquivalentTo(new StackyType.Function(
            new StackyType.Integer(true, SyntaxType.IntegerSize.S64),
            new StackyType.Void()));
        
        var anon = ((TypedExpression.Application)function.Body).Expressions[0];
        anon.Type.Should().BeEquivalentTo(new StackyType.Function(
            new StackyType.Integer(true, SyntaxType.IntegerSize.S64),
            new StackyType.Void()));

    }
    
    [Fact]
    public void AnonymousFunction_WithOutputs_ShouldInfer()
    {
        var program = ParseProgram(@"test () -> i64 { { 2 } invoke }");

        var typed = Infer(program);

        var function = typed.Functions.First();
        var expr = function.Body;
        expr.Type.Should().BeEquivalentTo(new StackyType.Function(
            new StackyType.Void(), 
            new StackyType.Integer(true, SyntaxType.IntegerSize.S64)));
        
        var anon = ((TypedExpression.Application)function.Body).Expressions[0];
        anon.Type.Should().BeEquivalentTo(new StackyType.Function(
            new StackyType.Void(), 
            new StackyType.Integer(true, SyntaxType.IntegerSize.S64)));
    }  
    
    [Fact]
    public void AnonymousFunction_WithInputsAndOutputs_ShouldInfer()
    {
        var program = ParseProgram(@"test i64 -> i64 str { { dup 1 + string } invoke }");

        var typed = Infer(program);

        var function = typed.Functions.First();
        var expr = function.Body;
        expr.Type.Should().BeEquivalentTo(new StackyType.Function(
            new StackyType.Integer(true, SyntaxType.IntegerSize.S64), 
            new StackyType.Composite(new StackyType[]
            {
                new StackyType.Integer(true, SyntaxType.IntegerSize.S64),
                new StackyType.String(),
            })));
        
        var anon = ((TypedExpression.Application)function.Body).Expressions[0];
        anon.Type.Should().BeEquivalentTo(new StackyType.Function(
            new StackyType.Integer(true, SyntaxType.IntegerSize.S64), 
            new StackyType.Composite(new StackyType[]
            {
                new StackyType.Integer(true, SyntaxType.IntegerSize.S64),
                new StackyType.String(),
            })));
    }
    
    [Fact]
    public void AnonymousFunctionPassedToString_ShouldError()
    {
        var program = ParseProgram("test () -> str { { 1 } string }");

        var func = () => Infer(program);

        func.Should().Throw<TypeInferenceException>();
    }
}