namespace Stacky.Parsing.Typing;

public class InferenceIntrinsicRegistry
{
    private readonly Dictionary<string, IInferenceIntrinsic> _map = new();

    public void Register(string name, IInferenceIntrinsic intrinsic)
    {
        _map.Add(name, intrinsic);
    }

    public bool TryGetIntrinsic(string name, out IInferenceIntrinsic? intrinsic) => _map.TryGetValue(name, out intrinsic);
}