using System.Collections.Immutable;
using System.Text;
using Stacky.Parsing.Typing;

namespace Stacky.Evaluation;

public class Evaluator
{
    private readonly TypedProgram _program;
    
    public EvaluationIntrinsicRegistry Intrinsics { get; } = new();

    public Evaluator(TypedProgram program)
    {
        _program = program;
    }

    public ImmutableStack<EvaluationValue> Run() => Run(ImmutableStack<EvaluationValue>.Empty);
    public ImmutableStack<EvaluationValue> Run(ImmutableStack<EvaluationValue> initial)
    {
        var state = new EvaluationState(_program, initial, ImmutableStack<IReadOnlyDictionary<string, EvaluationValue>>.Empty);
        var function = state.GetFunction("main");

        state = RunFunction(state, function);

        return state.Stack;
    }
    
    private EvaluationState RunFunction(EvaluationState state, TypedFunction function) => RunExpression(state, function.Body);

    public EvaluationState RunClosure(EvaluationState state, EvaluationValue.Closure closure)
    {
        state = state.PushBindings(closure.Bindings);
        state = RunExpression(state, closure.Body);
        state = state.PopBindings();
        
        return state;
    }

    
    public EvaluationState RunExpression(EvaluationState state, TypedExpression expr) =>
        expr switch
        {
            TypedExpression.LiteralInteger literal => state.Push(new EvaluationValue.Int64(literal.Value)),
            TypedExpression.LiteralString literal => state.Push(new EvaluationValue.String(Encoding.UTF8.GetBytes(literal.Value))),
            
            TypedExpression.Application application => RunApplication(state, application),
            TypedExpression.Binding binding => RunBinding(state, binding),
            TypedExpression.Closure closure => RunClosure(state, closure),
            TypedExpression.Identifier identifier => RunIdentifier(state, identifier),
            
            _ => throw new ArgumentOutOfRangeException(nameof(expr))
        };
    
    private EvaluationState RunApplication(EvaluationState state, TypedExpression.Application application) => application.Expressions.Aggregate(state, RunExpression);

    private EvaluationState RunClosure(EvaluationState state, TypedExpression.Closure closure)
    {
        var bindings = new Dictionary<string, EvaluationValue>();
        
        foreach (var (name, _) in closure.Bindings)
        {
            if (!state.TryLookupBinding(name, out var value))
            {
                throw new InvalidOperationException("failed to lookup binding for closure");
            }
            
            bindings.Add(name, value!);
        }
        
        return state.Push(new EvaluationValue.Closure(closure.Body, bindings));
    }

    private EvaluationState RunIdentifier(EvaluationState state, TypedExpression.Identifier identifier)
    {
        if (state.TryLookupBinding(identifier.Value, out var binding))
        {
            return state.Push(binding!);
        }
        
        if (identifier.Value.Length > 1)
        {
            switch (identifier.Value[0])
            {
                case '@': return RunInit(ref state, identifier);                
                case '#': return RunGetter(ref state, identifier);                
                case '~': return RunSetter(ref state, identifier);                
            }
        }

        if (Intrinsics.TryGetIntrinsic(identifier.Value, out var handler))
        {
            return handler!.Evaluate(this, state);
        }
        
        var function = state.GetFunction(identifier.Value);
        return RunFunction(state, function);

    }

    private EvaluationState RunBinding(EvaluationState state, TypedExpression.Binding binding)
    {
        var values = new Dictionary<string, EvaluationValue>();

        for (var i = binding.Names.Count - 1; i >= 0; i--)
        {
            var name = binding.Names[i];
            state = state.Pop(out var value);

            if (!values.ContainsKey(name.Value))
            {
                values[name.Value] = value;
            }
        }

        state = state.PushBindings(values);
        state = RunExpression(state, binding.Body);
        state = state.PopBindings();
        
        return state;
    }

    private static EvaluationState RunInit(ref EvaluationState state, TypedExpression.Identifier identifier)
    {
        var structName = identifier.Value[1..];
        var structDefinition = state.GetStruct(structName); 
        
        return state.Push(new EvaluationValue.Struct(structDefinition));
    }

    private static EvaluationState RunGetter(ref EvaluationState state, TypedExpression.Identifier identifier)
    {
        var fieldName = identifier.Value[1..];

        state = state.Pop(out var a);
        
        if (a is not EvaluationValue.Struct structValue)
        {
            throw new InvalidCastException($"Expected arg 0 to be Struct but got {a}");
        }

        var value = structValue.Get(fieldName);

        return state.Push(value);
    }

    private static EvaluationState RunSetter(ref EvaluationState state, TypedExpression.Identifier identifier)
    {
        var fieldName = identifier.Value[1..];

        state = state.Pop(out var value);
        state = state.Pop(out var a);
        
        if (a is not EvaluationValue.Struct structValue)
        {
            throw new InvalidCastException($"Expected arg 0 to be Struct but got {a}");
        }

        structValue = structValue.Update(fieldName, value);

        return state.Push(structValue);
    }
}