using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Stacky.Evaluation;
using Stacky.Intrinsics;
using Stacky.Parsing;
using Stacky.Parsing.Typing;

namespace Stacky.Tests;

public class SemanticsBase
{
    [Obsolete]
    protected IReadOnlyList<EvaluationValue> RunExpr(string code) => throw new NotImplementedException();

    
    protected IReadOnlyList<EvaluationValue> Run(string code) => Run(code, ImmutableStack<EvaluationValue>.Empty);

    private IReadOnlyList<EvaluationValue> Run(string code, ImmutableStack<EvaluationValue> initial)
    {
        var parser = new Parser("test", code);
        var program = parser.Parse();

        var inferer = new TypeInferer();
        All.Populate(inferer.Intrinsics);
        var typed = inferer.Infer(program);
        
        var evaluator = new Evaluator(typed);
        All.Populate(evaluator.Intrinsics);
        return evaluator.Run(initial).ToList();
    }
}