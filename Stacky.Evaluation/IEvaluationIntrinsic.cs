namespace Stacky.Evaluation;

public interface IEvaluationIntrinsic
{
    EvaluationState Evaluate(Evaluator evaluator, EvaluationState state);
}