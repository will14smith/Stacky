namespace Stacky.Parsing.Typing;

public abstract record StackySort
{
    public record Any : StackySort
    {
        public override bool IsCompatible(StackyType type) => true;
    }
    public record Stack : StackySort
    {
        public override bool IsCompatible(StackyType type) => true;

        public override string ToString()
        {
            return nameof(Stack);
        }
    }
    
    public record Composite(IReadOnlyCollection<StackySort> Sorts) : StackySort
    {
        public override bool IsCompatible(StackyType type) => Sorts.All(x => x.IsCompatible(type));

        public override string ToString()
        {
            return string.Join(", ", Sorts);
        }
    }

    public record Comparable : StackySort
    {
        public override bool IsCompatible(StackyType type) => type is StackyType.Integer;
    }
    public record Numeric : StackySort
    {
        public override bool IsCompatible(StackyType type) => type is StackyType.Integer;

        public override string ToString()
        {
            return nameof(Numeric);
        }
    }

    public record Printable : StackySort
    {
        public override bool IsCompatible(StackyType type) => type is StackyType.Boolean or StackyType.Integer or StackyType.String;
        
        public override string ToString()
        {
            return nameof(Printable);
        }
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