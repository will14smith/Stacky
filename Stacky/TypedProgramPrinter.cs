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
        PrintProgram($"struct ");
        PrintType(typedStruct.Type);
        PrintProgram(" {");
        Console.WriteLine();
        
        foreach (var field in typedStruct.Fields)
        {
            PrintProgram($"{field.Name.Value} ");
            PrintType(field.Type);
            Console.WriteLine();
        }
        
        PrintProgram("}");
        Console.WriteLine();
    }
    
    private static void Print(TypedFunction function)
    {
        PrintProgram($"{function.Name.Value} ");
        PrintType(function.Type);
        PrintProgram(" {");
        Console.WriteLine();

        Print(function.Body, 2);
        
        PrintProgram("}");
        Console.WriteLine();
    }

    private static void Print(TypedExpression expr, int indent)
    {
        switch (expr)
        {
            case TypedExpression.LiteralInteger literal:
                Indent(indent);
                PrintProgram($"{literal.Value} ");
                PrintType(literal.Type);
                Console.WriteLine();
                break;
            case TypedExpression.LiteralString literal:  
                Indent(indent);
                PrintProgram($"\"{literal.Value}\" ");
                PrintType(literal.Type);
                Console.WriteLine();
                break;

            case TypedExpression.Identifier identifier:
                Indent(indent);
                PrintProgram($"{identifier.Value} ");
                PrintType(identifier.Type);
                Console.WriteLine();
                break;
            
            case TypedExpression.Application application: 
                foreach (var childExpr in application.Expressions)
                {
                    Print(childExpr, indent);
                }
                break;
            
            case TypedExpression.Closure closure:
                Indent(indent);

                PrintProgram("[");

                foreach (var (name, type) in closure.Bindings)
                {
                    Console.WriteLine();
                    Indent(indent + 2);
                    PrintProgram($"{name} ");
                    PrintType(type);
                }

                if (closure.Bindings.Any())
                {
                    Console.WriteLine();
                }
                
                Indent(indent);
                PrintProgram("] ");
                
                
                PrintProgram("{");
                Console.WriteLine();

                Print(closure.Body, indent + 2);
                
                Indent(indent);
                PrintProgram("} ");

                if (closure.Type is not StackyType.Function functionType)
                {
                    throw new InvalidOperationException();
                }

                PrintType(functionType.Output);
                Console.WriteLine();
                break;
            
            case TypedExpression.Binding binding:
                Indent(indent);
                PrintProgram("(");
                Console.WriteLine();

                foreach (var name in binding.Names)
                {
                    Indent(indent + 2);
                    PrintProgram($"{name.Value} ");
                    PrintType(name.Type);
                    Console.WriteLine();
                }
                
                Indent(indent);
                PrintProgram(")");
                Console.WriteLine();

                Indent(indent);
                PrintProgram("{");
                Console.WriteLine();

                Print(binding.Body, indent + 2);

                Indent(indent);
                PrintProgram("}");
                Console.WriteLine();

                break;
            
            default: throw new ArgumentOutOfRangeException(nameof(expr));
        }
    }

    private static void Indent(int indent)
    {
        for(var i = 0; i < indent; i++) Console.Write(' ');
    }
    
    private static void PrintProgram(object text)
    {
        Console.ResetColor();
        Console.Write(text.ToString());
    }
    private static void PrintType(StackyType type)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        
        switch (type)
        {
            case StackyType.Composite composite:
                PrintType(composite.Left);
                Console.Write(" ");
                PrintType(composite.Right);
                break;
            
            case StackyType.Function function:
                Console.Write("(");
                PrintType(function.Input);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" -> ");
                PrintType(function.Output);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(")");
                break;
            
            case StackyType.Struct @struct:
                Console.Write(@struct.Name);
                break;

            default: 
                Console.Write(type);
                break;
        }
        
        Console.ResetColor();
    }
}

public class TablePrinter
{
    private readonly List<(Cell Left , Cell Right)> _rows = new();

    public void Add(Cell left, Cell right) => _rows.Add((left, right));

    public void Print(int indent)
    {
        var maxLeft = _rows.Max(x => x.Left.Text.Length);

        foreach (var (left, right) in _rows)
        {
            Pad(indent);
            Print(left);
            Pad(maxLeft - left.Text.Length + 1);
            Print(right);
            
        }
    }

    private void Print(Cell cell)
    {
        Console.ResetColor();
        if (cell.Color.HasValue) Console.ForegroundColor = cell.Color.Value;
        Console.Write(cell.Text);
    }

    private void Pad(int indent) { for(var i = 0; i < indent; i++) Console.Write(' '); }

    public record Cell(ConsoleColor? Color, string Text);
}