namespace Stacky.Evaluation;

public class EvaluationIntrinsicRegistry
{
    private readonly Dictionary<string, IEvaluationIntrinsic> _map = new();

    public void Register(string name, IEvaluationIntrinsic intrinsic)
    {
        _map.Add(name, intrinsic);
    }

    public bool TryGetIntrinsic(string name, out IEvaluationIntrinsic? intrinsic) => _map.TryGetValue(name, out intrinsic);
}