using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public class TypeInferer
{
    public static TypedProgram Infer(SyntaxProgram program)
    {
        var (built, state) = InferenceBuilder.Build(program);
        
        var substitutions = InferenceSolver.Solve(state);
        
        return substitutions.Apply(built);
    }
}