﻿using Stacky.Language.Compilation;
using Stacky.Language.Evaluation;
using Stacky.Language.Parsing;



// var input = @"() main () { ""aaaaaaaaaaaaaaaaaaaaaaaaaaaa"" print }";
// var input = @"() main () { 1 2 + print }";
// var input = @"() main () { 1 2 + toString ""a"" concat print }";
var input = @"
() main () {
    1 mul3 2 + toString ""a"" concat print
}

i64 mul3 i64 { 3 * }
";

var parser = new Parser("input.st", input);
var program = parser.Parse();

var evaluator = new Evaluator(program);
evaluator.Run();

var compiler = new Compiler(program);
compiler.Compile();