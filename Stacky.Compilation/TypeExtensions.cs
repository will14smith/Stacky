using Stacky.Parsing.Typing;

namespace Stacky.Compilation;

public static class TypeExtensions
{
    public static StackyType LastOutput(this StackyType type)
    {
        if (type is not StackyType.Function func)
        {
            return type;
        }
        
        if (func.Output is StackyType.Composite comp)
        {
            return comp.Right;
        }
            
        return func.Output;

    }
}