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

    public sealed record Struct(string Name, IReadOnlyList<(string Name, StackyType Type)> Fields) : StackyType
    {
        public bool Equals(Struct? other) => other != null && other.Name == Name;
        public override int GetHashCode() => Name.GetHashCode();

    }

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
            Void => Array.Empty<StackyType>(),
            Composite comp => comp.Types,
            _ => new[] { type }
        };
    }

    public record Getter(StackyType StructType, string FieldName) : StackyType;
    public record Setter(StackyType StructType, string FieldName) : StackyType;

    public static StackyType MakeGetter(StackyType structType, string fieldName)
    {
        if (structType is Struct targetStruct)
        {
            var field = targetStruct.Fields.First(x => x.Name == fieldName);
            return field.Type;
        }

        return new Getter(structType, fieldName);
    }
    public static StackyType MakeSetter(StackyType structType, string fieldName)
    {
        if (structType is Struct targetStruct)
        {
            var field = targetStruct.Fields.First(x => x.Name == fieldName);
            return field.Type;
        }

        return new Getter(structType, fieldName);
    }
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

    public record Gettable(string Field) : StackySort
    {
        public override bool IsCompatible(StackyType type) => type is StackyType.Struct structType && structType.Fields.Any(x => x.Name == Field);
    }
    public record Settable(string Field) : StackySort
    {
        public override bool IsCompatible(StackyType type) => type is StackyType.Struct structType && structType.Fields.Any(x => x.Name == Field);
    }
    
    public static StackySort MakeComposite(params StackySort[] sorts)
    {
        var flat = sorts.SelectMany(x => x is Composite comp ? comp.Sorts : new[] { x }).Where(x => x is not Any).ToList();

        var distinct = flat.Distinct().ToList();

        return distinct.Count switch
        {
            0 => new Any(),
            1 => distinct[0],
            _ => new Composite(distinct)
        };
    }

    public abstract bool IsCompatible(StackyType type);
}