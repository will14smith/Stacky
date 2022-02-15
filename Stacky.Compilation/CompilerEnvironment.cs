using Stacky.Compilation.LLVM;

namespace Stacky.Compilation;

public class CompilerEnvironment
{
    private readonly Dictionary<string, CompilerValue> _functions = new();
    private readonly Dictionary<string, CompilerStruct> _structs = new();
    
    private readonly CompilerEmitter _emitter;

    public CompilerEnvironment(CompilerEmitter emitter)
    {
        _emitter = emitter;
    }

    public CompilerValue GetFunction(string name)
    {
        return _functions[name];
    }  
    
    public CompilerStruct GetStruct(string name)
    {
        return _structs[name];
    }

    public CompilerValue DefineFunction(string name, CompilerType.Function type)
    {
        var value = _emitter.DefineFunction(name, type);
        _functions.Add(name, value);
        return value;
    } 
    public CompilerValue DefineFunction(string name, NativeFunction type)
    {
        var value = _emitter.DefineNativeFunction(name, type);
        _functions.Add(name, value);
        return value;
    }

    public CompilerStruct DefineStruct(string name, CompilerType.Struct type)
    {
        var value = _emitter.DefineStruct(name, type);
        _structs.Add(name, value);
        return value;
    }
}