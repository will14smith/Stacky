using Stacky.Parsing.Typing;

namespace Stacky;

public class TypedProgramPrinter
{
    public static void Print(TypedProgram program)
    {
        foreach (var typedStruct in program.Structs) { Print(typedStruct); }
        foreach (var function in program.Functions) { Print(function); }
    }
    
    private static void Print(TypedStruct typedStruct)
    {
        throw new NotImplementedException();
    }
    
    private static void Print(TypedFunction function)
    {
        ProgramColor();
        Console.Write($"{function.Name.Value} ");
        TypeColor();
        Console.Write("TODO");
        ProgramColor();
        Console.WriteLine(" {");

        ProgramColor();
        Console.WriteLine("}");
    }
    
    private static void ProgramColor()
    {
        Console.ResetColor();
    }
    private static void TypeColor()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
    }
}