using System.Text;
using Stacky.Parsing.Syntax;

namespace Stacky.Evaluation;

public abstract record EvaluationValue
{
    public record Boolean(bool Value) : EvaluationValue;
    public record Int64(long Value) : EvaluationValue;

    public record String(byte[] Value) : EvaluationValue
    {
        public string StringValue => Encoding.UTF8.GetString(Value);
    }

    public record Function(SyntaxExpression Body) : EvaluationValue;

    public record Struct(IReadOnlyDictionary<string, EvaluationValue?> Fields) : EvaluationValue
    {
        public Struct(SyntaxStruct structDefinition) : this(structDefinition.Fields.ToDictionary(x => x.Name.Value, x => (EvaluationValue?)null)) { }

        public EvaluationValue Get(string name)
        {
            if (!Fields.TryGetValue(name, out var value))
            {
                throw new Exception($"Invalid field '{name}'");
            }

            if (value == null)
            {
                throw new NotImplementedException("TODO handle zero initialised values");
            }

            return value;
        }
        
        public Struct Update(string name, EvaluationValue value)
        {
            if (!Fields.ContainsKey(name))
            {
                throw new Exception($"Invalid field '{name}'");
            }

            var newFields = Fields.ToDictionary(x => x.Key, x => x.Key == name ? value : x.Value);
            
            return new Struct(newFields);
        }
    }
}