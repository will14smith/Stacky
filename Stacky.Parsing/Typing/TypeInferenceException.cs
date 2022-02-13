namespace Stacky.Parsing.Typing;

[Serializable]
public class TypeInferenceException : Exception
{
    // TODO track syntax position
    
    public TypeInferenceException(string message) : base(message) { }
}

public class InvalidCastTypeInferenceException : TypeInferenceException
{
    public StackyType Left { get; }
    public StackyType Right { get; }

    public InvalidCastTypeInferenceException(StackyType left, StackyType right) : base($"Unable to find cast between '{left}' and '{right}'")
    {
        Left = left;
        Right = right;
    }
}

public class InvalidSortTypeInferenceException : TypeInferenceException
{
    public StackySort Sort { get; }
    public StackyType Type { get; }

    public InvalidSortTypeInferenceException(StackySort sort, StackyType type) : base($"Type '{type}' is not compatible with sort '{sort}'")
    {
        Sort = sort;
        Type = type;
    }
}