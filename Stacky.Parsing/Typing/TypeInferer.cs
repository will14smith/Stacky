using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public class TypeInferer
{
    public InferenceIntrinsicRegistry Intrinsics { get; } = new();

    public TypedProgram Infer(SyntaxProgram program)
    {
        var builder = new InferenceBuilder(Intrinsics);
        var (built, state) = builder.Build(program);
        
        var substitutions = InferenceSolver.Solve(state);
        
        return substitutions.Apply(built);
    }
}