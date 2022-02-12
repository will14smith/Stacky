using Stacky.Compilation.LLVM;

namespace Stacky.Compilation;

public class CompilerIntrinsics
{
    private delegate CompilerStack Intrinsic(CompilerStack stack);

    private readonly CompilerAllocator _allocator;
    private readonly CompilerEmitter _emitter;
    
    private readonly Lazy<CompilerValue> _sprintf;
    private readonly Lazy<CompilerValue> _strlen;
    private readonly Lazy<CompilerValue> _strcat;
    private readonly Lazy<CompilerValue> _printf;
    
    private readonly IReadOnlyDictionary<string, Intrinsic> _map;

    public CompilerIntrinsics(CompilerAllocator allocator, CompilerEmitter emitter)
    {
        _allocator = allocator;
        _emitter = emitter;
        
        _sprintf = new Lazy<CompilerValue>(() => _emitter.DefineNativeFunction("sprint", _emitter.NativeFunctions.Sprintf));
        _strlen = new Lazy<CompilerValue>(() => _emitter.DefineNativeFunction("strlen", _emitter.NativeFunctions.Strlen));
        _strcat = new Lazy<CompilerValue>(() => _emitter.DefineNativeFunction("strcat", _emitter.NativeFunctions.Strcat));
        _printf = new Lazy<CompilerValue>(() => _emitter.DefineNativeFunction("printf", _emitter.NativeFunctions.Printf));

        _map = new Dictionary<string, Intrinsic>
        {
            { "+", Add },
            { "*", Mul },
            { "concat", StringConcat },
            { "toString", ToString },
            { "print", Print },
        };
    }
    
    public bool TryCompile(string name, ref CompilerStack stack)
    {
        if (!_map.TryGetValue(name, out var handler))
        {
            return false;
        }
        
        stack = handler(stack);
        return true;
    }
    
    private CompilerStack Add(CompilerStack stack)
    {
        stack = stack.Pop<CompilerType.Long>(out var b, out _);
        stack = stack.Pop<CompilerType.Long>(out var a, out _);

        return stack.Push(_emitter.Add(a, b));
    }  
    private CompilerStack Mul(CompilerStack stack)
    {
        stack = stack.Pop<CompilerType.Long>(out var b, out _);
        stack = stack.Pop<CompilerType.Long>(out var a, out _);

        return stack.Push(_emitter.Mul(a, b));
    }

    private CompilerStack StringConcat(CompilerStack stack)
    {
        // get length for new buffer
        stack = stack.Pop<CompilerType.String>(out var b, out var removeRootB);
        stack = stack.Pop<CompilerType.String>(out var a, out var removeRootA);

        var alength = _emitter.Call(_strlen.Value, new CompilerType.Long(), a);
        var blength = _emitter.Call(_strlen.Value, new CompilerType.Long(), b);

        // allocate memory
        var bufferLength = _emitter.Add(_emitter.Add(alength, blength), _emitter.Literal(1));
        var buffer = _allocator.AllocateRaw(new CompilerType.String(), bufferLength);

        // concat
        _emitter.Call(_strcat.Value, new CompilerType.String(), buffer, a);
        removeRootB();

        _emitter.Call(_strcat.Value, new CompilerType.String(), buffer, b);
        removeRootA();

        // push to stack
        return stack.Push(buffer);
    }

    private CompilerStack ToString(CompilerStack stack)
    {
        var type = stack.PeekType();

        return type switch
        {
            CompilerType.Long => ToStringLong(stack),
            CompilerType.String => stack,

            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private CompilerStack ToStringLong(CompilerStack stack)
    {
        // allocate memory
        var bufferLength = (uint)(long.MinValue.ToString().Length + 1);
        var buffer = _allocator.AllocateRaw(new CompilerType.String(), bufferLength);

        // format long -> string
        var format = _emitter.Literal("%lld");
        stack = stack.Pop<CompilerType.Long>(out var value, out _);
        _emitter.Call(_sprintf.Value, new CompilerType.String(), buffer, format, value);
        
        // push to stack
        return stack.Push(buffer);
    }

    private CompilerStack Print(CompilerStack stack)
    {
        var type = stack.PeekType();

        switch (type)
        {
            case CompilerType.Long: return PrintLong(stack);
            case CompilerType.String: return PrintString(stack);

            default: throw new ArgumentOutOfRangeException(nameof(type));
        }
    }

    private CompilerStack PrintLong(CompilerStack stack)
    {
        var format = _emitter.Literal("%lld");
        stack = stack.Pop<CompilerType.Long>(out var value, out _);

        // TODO not a Long... 
        _emitter.Call(_printf.Value, new CompilerType.Long(), format, value);
        
        return stack;
    }

    private CompilerStack PrintString(CompilerStack stack)
    {
        var format = _emitter.Literal("%s");
        stack = stack.Pop<CompilerType.String>(out var value, out var removeRoot);

        // TODO not a Long... 
        _emitter.Call(_printf.Value, new CompilerType.Long(), format, value);
        removeRoot();
        
        return stack;
    }
}