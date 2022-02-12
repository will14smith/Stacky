using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Stacky.Evaluation;
using Stacky.Parsing;

namespace Stacky.Tests;

public class SemanticsBase
{
    protected IReadOnlyList<EvaluationValue> RunExpr(string exprCode) => RunExpr(exprCode, ImmutableStack<EvaluationValue>.Empty);

    private IReadOnlyList<EvaluationValue> RunExpr(string exprCode, ImmutableStack<EvaluationValue> initial)
    {
        // TODO this type is wrong...
        return Run($"main () -> () {{ {exprCode} }}", initial);
    }

    protected IReadOnlyList<EvaluationValue> Run(string code) => Run(code, ImmutableStack<EvaluationValue>.Empty);

    private IReadOnlyList<EvaluationValue> Run(string code, ImmutableStack<EvaluationValue> initial)
    {
        var parser = new Parser("test", code);
        var program = parser.Parse();

        return new Evaluator(program).Run(initial).ToList();
    }
}