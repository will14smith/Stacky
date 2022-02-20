using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public class TypeInferer
{
    public InferenceIntrinsicRegistry Intrinsics { get; } = new();

    public (TypedProgram Program, InferenceState State) Annotate(SyntaxProgram program)
    {
        var builder = new InferenceBuilder(Intrinsics);
        return builder.Build(program);
    }  
    
    public TypedProgram Infer(SyntaxProgram program)
    {
        var (built, state) = Annotate(program);
        var substitutions = InferenceSolver.Solve(state);
        return substitutions.Apply(built);
    }
}