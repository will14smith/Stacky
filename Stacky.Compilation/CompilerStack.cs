using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using Stacky.Compilation.LLVM;

namespace Stacky.Compilation;

public class CompilerStack
{
    private readonly CompilerAllocator _allocator;
    private readonly CompilerEmitter _emitter;

    private readonly ImmutableStack<CompilerType> _stack;
    private readonly ImmutableStack<IReadOnlyDictionary<string, CompilerValue>> _bindings;

    public CompilerStack(CompilerAllocator allocator, CompilerEmitter emitter) : this(allocator, emitter, ImmutableStack<CompilerType>.Empty, ImmutableStack<IReadOnlyDictionary<string, CompilerValue>>.Empty) { }
    private CompilerStack(CompilerAllocator allocator, CompilerEmitter emitter, ImmutableStack<CompilerType> stack, ImmutableStack<IReadOnlyDictionary<string, CompilerValue>> bindings)
    {
        _allocator = allocator;
        _emitter = emitter;
        _stack = stack;
        _bindings = bindings;
    }

    [Pure]
    public CompilerStack PushType(CompilerType type)
    {
        return WithStack(_stack.Push(type));
    }
    
    [Pure]
    public CompilerType PeekType()
    {
        return _stack.Peek();
    }

    [Pure]
    public CompilerValue Peek(int depth = 0)
    {
        var types = _stack.Take(depth + 1).ToArray();
        return _emitter.Peek(types);
    }
    
    [Pure]
    public CompilerStack PopType(out CompilerType type)
    {
        return WithStack(_stack.Pop(out type));
    }
    
    [Pure]
    public CompilerStack Push(CompilerValue value)
    {
        _allocator.AddRoot(value);
        _emitter.Push(value);
        
        return PushType(value.Type);
    }

    [Pure]
    public CompilerStack Pop(out CompilerValue value, out Action removeRoot)
    {
        var stack = PopType(out var type);

        var localValue = value = _emitter.Pop(type);
        removeRoot = () => { _allocator.RemoveRoot(localValue); };
        
        return stack;
    }
    
    [Pure]
    public bool TryGetBinding(string name, out CompilerValue value)
    {
        foreach (var bindings in _bindings)
        {
            if (bindings.TryGetValue(name, out value))
            {
                return true;
            }
        }

        value = default;
        return false;
    }

    
    [Pure]
    public CompilerStack PushBindings(Dictionary<string, CompilerValue> bindings) => WithBindings(_bindings.Push(bindings));

    [Pure]
    public CompilerStack PopBindings() => WithBindings(_bindings.Pop());

    [Pure]
    private CompilerStack WithStack(ImmutableStack<CompilerType> stack) => new(_allocator, _emitter, stack, _bindings);
    [Pure]
    private CompilerStack WithBindings(ImmutableStack<IReadOnlyDictionary<string, CompilerValue>> bindings) => new(_allocator, _emitter, _stack, bindings);
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