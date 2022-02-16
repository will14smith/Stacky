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
            { "drop", Drop },
            { "dup", Duplicate },
            { "swap", Swap },
            
            { "+", Add },
            { "-", Sub },
            { "*", Mul },
            { ">", Greater },
            
            { "concat", StringConcat },
            { "string", String },
            { "print", Print },
            { "invoke", Invoke },
            
            { "if", If },
            { "if-else", IfElse },
            { "while", While },
            
            { "true", True },
            { "false", False },
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
    
    private CompilerStack Drop(CompilerStack stack)
    {
        stack = stack.Pop(out _, out var removeRoot);
        removeRoot();
        return stack;
    }

    private CompilerStack Duplicate(CompilerStack stack)
    {
        stack = stack.Pop(out var value, out var removeRoot);
        stack = stack.Push(value);
        stack = stack.Push(value);
        removeRoot();
        return stack;
    }
    
    private CompilerStack Swap(CompilerStack stack)
    {
        stack = stack.Pop(out var b, out var removeRootB);
        stack = stack.Pop(out var a, out var removeRootA);

        stack = stack.Push(b);
        removeRootB();

        stack = stack.Push(a);
        removeRootA();

        return stack;
    }

    private CompilerStack Add(CompilerStack stack)
    {
        stack = stack.Pop<CompilerType.Long>(out var b, out _);
        stack = stack.Pop<CompilerType.Long>(out var a, out _);

        return stack.Push(_emitter.Add(a, b));
    }  
    private CompilerStack Sub(CompilerStack stack)
    {
        stack = stack.Pop<CompilerType.Long>(out var b, out _);
        stack = stack.Pop<CompilerType.Long>(out var a, out _);

        return stack.Push(_emitter.Sub(a, b));
    }  
    private CompilerStack Mul(CompilerStack stack)
    {
        stack = stack.Pop<CompilerType.Long>(out var b, out _);
        stack = stack.Pop<CompilerType.Long>(out var a, out _);

        return stack.Push(_emitter.Mul(a, b));
    }  
    private CompilerStack Greater(CompilerStack stack)
    {
        stack = stack.Pop<CompilerType.Long>(out var b, out _);
        stack = stack.Pop<CompilerType.Long>(out var a, out _);

        return stack.Push(_emitter.Greater(a, b));
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

    private CompilerStack String(CompilerStack stack)
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
        var format = _emitter.Literal("%lld\n");
        stack = stack.Pop<CompilerType.Long>(out var value, out _);

        // TODO not a Long... 
        _emitter.Call(_printf.Value, new CompilerType.Long(), format, value);
        
        return stack;
    }

    private CompilerStack PrintString(CompilerStack stack)
    {
        var format = _emitter.Literal("%s\n");
        stack = stack.Pop<CompilerType.String>(out var value, out var removeRoot);

        // TODO not a Long... 
        _emitter.Call(_printf.Value, new CompilerType.Long(), format, value);
        removeRoot();
        
        return stack;
    }
    
    private CompilerStack Invoke(CompilerStack stack)
    {
        stack = stack.Pop<CompilerType.Function>(out var value, out _);

        return ExpressionCompiler.CallFunction(_emitter, stack, value);
    }
    
    private CompilerStack If(CompilerStack stack)
    {
        stack = stack.Pop<CompilerType.Function>(out var trueFunc, out _);
        stack = stack.Pop<CompilerType.Boolean>(out var condition, out _);

        var mergeBlock = _emitter.CreateBlockInCurrent("merge");
        var trueBlock = _emitter.CreateBlockInCurrent("true");

        _emitter.Branch(condition, trueBlock, mergeBlock);
        
        _emitter.BeginBlock(trueBlock);
        var trueStack = ExpressionCompiler.CallFunction(_emitter, stack, trueFunc);
        _emitter.Branch(mergeBlock);
        
        _emitter.BeginBlock(mergeBlock);
        
        // TODO assuming the true/not-taken stack are the same for now - typing inference will probably confirm this?
        return trueStack;
    }

    private CompilerStack IfElse(CompilerStack stack)
    {
        stack = stack.Pop<CompilerType.Function>(out var falseFunc, out _);
        stack = stack.Pop<CompilerType.Function>(out var trueFunc, out _);
        stack = stack.Pop<CompilerType.Boolean>(out var condition, out _);

        var mergeBlock = _emitter.CreateBlockInCurrent("merge");
        var trueBlock = _emitter.CreateBlockInCurrent("true");
        var falseBlock = _emitter.CreateBlockInCurrent("false");

        _emitter.Branch(condition, trueBlock, falseBlock);
        
        _emitter.BeginBlock(trueBlock);
        var trueStack = ExpressionCompiler.CallFunction(_emitter, stack, trueFunc);
        _emitter.Branch(mergeBlock);
        
        _emitter.BeginBlock(falseBlock);
        var falseStack = ExpressionCompiler.CallFunction(_emitter, stack, falseFunc);
        _emitter.Branch(mergeBlock);
        
        _emitter.BeginBlock(mergeBlock);
        
        // TODO assuming the true/false stack are the same for now - typing inference will probably confirm this?
        return trueStack;
    }

    private CompilerStack While(CompilerStack stack)
    {
        stack = stack.Pop<CompilerType.Function>(out var loopFunc, out _);
        stack = stack.Pop<CompilerType.Function>(out var condFunc, out _);

        var doneBlock = _emitter.CreateBlockInCurrent("done");
        var condBlock = _emitter.CreateBlockInCurrent("cond");
        var loopBlock = _emitter.CreateBlockInCurrent("loop");

        _emitter.Branch(condBlock);
        
        _emitter.BeginBlock(loopBlock);
        var loopStack = ExpressionCompiler.CallFunction(_emitter, stack, loopFunc);
        _emitter.Branch(condBlock);
        
        _emitter.BeginBlock(condBlock);
        var condStack = ExpressionCompiler.CallFunction(_emitter, stack, condFunc);
        condStack = condStack.Pop<CompilerType.Boolean>(out var condition, out _);
        _emitter.Branch(condition, loopBlock, doneBlock);

        _emitter.BeginBlock(doneBlock);

        // TODO assuming the cond/loop stacks aren't updating things - typing inference will probably confirm this?
        return stack;
    }

    private CompilerStack True(CompilerStack stack) => stack.Push(_emitter.Literal(true));
    private CompilerStack False(CompilerStack stack) => stack.Push(_emitter.Literal(false));
}