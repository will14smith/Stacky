using Stacky.Language.Compilation.LLVM;

namespace Stacky.Language.Compilation;

public class CompilerEnvironment
{
    private readonly Dictionary<string, CompilerValue> _functions = new();
    private readonly CompilerEmitter _emitter;

    public CompilerEnvironment(CompilerEmitter emitter)
    {
        _emitter = emitter;
    }

    public CompilerValue GetFunction(string name)
    {
        return _functions[name];
    }

    public CompilerValue DefineFunction(string name, CompilerType.Function type)
    {
        var value = _emitter.DefineFunction(name, type);
        
        _functions.Add(name, value);
        return _functions[name];
    } 
    public CompilerValue DefineFunction(string name, NativeFunction type)
    {
        var value = _emitter.DefineNativeFunction(name, type);
        
        _functions.Add(name, value);
        return _functions[name];
    }
    
}