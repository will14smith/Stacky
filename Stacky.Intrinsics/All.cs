using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics;

public class All
{
    private static readonly IReadOnlyCollection<IIntrinsic> Intrinsics;

    static All()
    {
        var assembly = typeof(All).Assembly;
        var types = assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsAssignableTo(typeof(IIntrinsic)));
        
        Intrinsics = types.Select(type => (IIntrinsic) Activator.CreateInstance(type)!).ToList();
    }

    public static void Populate(InferenceIntrinsicRegistry registry)
    {
        foreach (var intrinsic in Intrinsics)
        {
            registry.Register(intrinsic.Name, intrinsic);
        }
    }
    
    public static void Populate(EvaluationIntrinsicRegistry registry)
    {
        foreach (var intrinsic in Intrinsics)
        {
            registry.Register(intrinsic.Name, intrinsic);
        }
    }

    public static void Populate(CompilerIntrinsicRegistry registry)
    {
        foreach (var intrinsic in Intrinsics)
        {
            registry.Register(intrinsic.Name, intrinsic);
        }
    }
}