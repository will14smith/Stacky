using System.Diagnostics;
using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public abstract record StackyType
{
    public record Variable(int Id, StackySort Sort) : StackyType
    {
        public override string ToString()
        {
            return Sort is StackySort.Any ? $"[Id={Id}]" : $"[Id={Id}, Sort={Sort}]";
        }
    }
    
    public record Boolean : StackyType;
    public record Integer(bool Signed, SyntaxType.IntegerSize Size) : StackyType;
    public record String : StackyType;

    public record Void : StackyType;

    public record Composite(IReadOnlyList<StackyType> Types) : StackyType
    {
        public override string ToString()
        {
            return $"[{string.Join(", ", Types)}]";
        }
    }
    public record Function(StackyType Input, StackyType Output) : StackyType;

    public static StackyType MakeComposite(params StackyType[] types) =>
        types.Length switch
        {
            0 => new Void(),
            1 => types[0],
            _ => new Composite(FlattenComposites(types))
        };
    private static IReadOnlyList<StackyType> FlattenComposites(IEnumerable<StackyType> types) => types.SelectMany(Iterator).ToList();
    
    public static IEnumerable<StackyType> Iterator(StackyType type)
    {
        return type switch
        {
            Composite comp => comp.Types,
            Void => Array.Empty<StackyType>(),
            _ => new[] { type }
        };
    }
    
    // public record Apply(StackyType Base, StackyType Remove, StackyType Add) : StackyType
    // {
    //     public override string ToString()
    //     {
    //         return $"({Base} - {Remove}) + {Add}";
    //     }
    // }
}

public abstract record StackySort
{
    public record Any : StackySort
    {
        public override bool IsCompatible(StackyType type) => true;
    }
    public record Composite(IReadOnlyCollection<StackySort> Sorts) : StackySort
    {
        public override bool IsCompatible(StackyType type) => Sorts.All(x => x.IsCompatible(type));
    }

    public record Comparable : StackySort
    {
        public override bool IsCompatible(StackyType type) => type is StackyType.Integer;
    }
    public record Numeric : StackySort
    {
        public override bool IsCompatible(StackyType type) => type is StackyType.Integer;
    }

    public record Printable : StackySort
    {
        public override bool IsCompatible(StackyType type) => type is StackyType.Boolean or StackyType.Integer or StackyType.String;
    }

    public static StackySort MakeComposite(params StackySort[] sorts)
    {
        // TODO handle de-duplication
        return new Composite(sorts);
    }

    public abstract bool IsCompatible(StackyType type);
}