using System.Collections.Immutable;
using Stacky.Compilation.LLVM;

namespace Stacky.Compilation;

public class CompilerStack
{
    private readonly CompilerAllocator _allocator;
    private readonly CompilerEmitter _emitter;

    private readonly ImmutableStack<CompilerType> _stack;

    public CompilerStack(CompilerAllocator allocator, CompilerEmitter emitter) : this(allocator, emitter, ImmutableStack<CompilerType>.Empty) { }
    private CompilerStack(CompilerAllocator allocator, CompilerEmitter emitter, ImmutableStack<CompilerType> stack)
    {
        _stack = stack;
        _allocator = allocator;
        _emitter = emitter;
    }

    public CompilerStack PushType(CompilerType type)
    {
        return WithStack(_stack.Push(type));
    }
    
    public CompilerType PeekType()
    {
        return _stack.Peek();
    }

    public CompilerValue Peek(int depth = 0)
    {
        var types = _stack.Take(depth + 1).ToArray();
        return _emitter.Peek(types);
    }
    
    public CompilerStack PopType(out CompilerType type)
    {
        return WithStack(_stack.Pop(out type));
    }
    
    public CompilerStack Push(CompilerValue value)
    {
        _allocator.AddRoot(value);
        _emitter.Push(value);
        
        return PushType(value.Type);
    }

    public CompilerStack Pop(out CompilerValue value, out Action removeRoot)
    {
        var stack = PopType(out var type);

        var localValue = value = _emitter.Pop(type);
        removeRoot = () => { _allocator.RemoveRoot(localValue); };
        
        return stack;
    }
    
    private CompilerStack WithStack(ImmutableStack<CompilerType> stack) => new(_allocator, _emitter, stack);
}

public static class CompilerStackExtensions
{
    public static CompilerStack Pop<T>(this CompilerStack stack, out CompilerValue value, out Action removeRoot)
    {
        stack = stack.Pop(out value, out removeRoot);

        if (value.Type is not T)
        {
            throw new InvalidCastException($"unable to cast '{value.Type}' to '{typeof(T)}'");
        }
        
        return stack;
    }
}