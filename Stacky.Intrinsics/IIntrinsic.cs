using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing.Typing;

namespace Stacky.Intrinsics;

public interface IIntrinsic : IInferenceIntrinsic, IEvaluationIntrinsic, ICompilerIntrinsic
{
    string Name { get; }
}