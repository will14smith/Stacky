using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public abstract record StackyType
{
    public record Variable(int Id, StackySort? Sort) : StackyType;
    
    public record Boolean : StackyType;
    public record Integer(bool Signed, SyntaxType.IntegerSize Size) : StackyType;
    public record String : StackyType;

    public record Function(IReadOnlyList<StackyType> Input, IReadOnlyList<StackyType> Output) : StackyType;

}

public abstract record StackySort
{
    public record Comparable : StackySort;
    public record Numeric : StackySort;
    public record Printable : StackySort;
}