using Stacky.Evaluation;

namespace Stacky.Intrinsics.Io;

public record FileEvaluationValue(FileStream Stream) : EvaluationValue;