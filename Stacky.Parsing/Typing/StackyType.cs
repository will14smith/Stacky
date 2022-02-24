using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public abstract record StackyType
{
    public record Variable(int Id, StackySort Sort) : StackyType
    {
        public override string ToString()
        {
            return Sort is StackySort.Any ? $"T{Id}" : $"({Sort} T{Id})";
        }
    }
    
    public record Boolean : StackyType;

    public record Integer(bool Signed, SyntaxType.IntegerSize Size) : StackyType
    {
        public override string ToString()
        {
            return $"{(Signed ? "i" : "u")}{Size.ToString()[1..]}";
        }
    }

    public record String : StackyType
    {
        public override string ToString()
        {
            return "str";
        }
    }

    public record Void : StackyType
    {
        public override string ToString()
        {
            return nameof(Void);
        }
    }

    public record Composite(StackyType Left, StackyType Right) : StackyType
    {
        public override string ToString()
        {
            return $"[{Left}, {Right}]";
        }
    }

    public record Function(StackyType Input, StackyType Output) : StackyType
    {
        public override string ToString()
        {
            return $"({Input} -> {Output})";
        }
    }

    public sealed record Struct(string Name, IReadOnlyList<StructField> Fields) : StackyType
    {
        public bool Equals(Struct? other) => other != null && other.Name == Name;
        public override int GetHashCode() => Name.GetHashCode();
    }
    public record StructField(string Name, StackyType Type);

    public static StackyType MakeComposite(params StackyType[] types)
    {
        var flat = FlattenComposites(types);

        if (flat.Count == 0) return new Void();

        var output = flat[0];
        for (var i = 1; i < flat.Count; i++)
        {
            output = new Composite(output, flat[i]);
        }
        return output;
    }

    private static IReadOnlyList<StackyType> FlattenComposites(IEnumerable<StackyType> types) => types.SelectMany(Iterator).ToList();
    
    public static IEnumerable<StackyType> Iterator(StackyType type)
    {
        return type switch
        {
            Composite comp => Iterator(comp.Left).Concat(Iterator(comp.Right)),
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

        return new Setter(structType, fieldName);
    }
}