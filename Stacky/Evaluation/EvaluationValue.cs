namespace Stacky.Evaluation;

public abstract record EvaluationValue
{
    public record Int64(long Value) : EvaluationValue;
    public record String(string Value) : EvaluationValue;
}