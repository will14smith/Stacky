using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Parsing;
using Stacky.Parsing.Typing;

// var input = @"main () -> () { ""aaaaaaaaaaaaaaaaaaaaaaaaaaaa"" print }";
// var input = @"main () -> () { 1 2 + print }";
// var input = @"main () -> () { 1 2 + string ""a"" concat print }";
// var input = @"
// main () -> () {
//     1 mul3 2 + string ""a"" concat print
// }
//
// mul3 i64 -> i64 { 3 * }
// ";
// var input = @"main () -> () { 1 { 1 + } invoke print }";
// var input = "main () -> () { true { 1 print } if }";
// var input = "main () -> () { 1 false { 2 + } { 1 + } if-else print }";
// var input = "main () -> () { 1 false { 2 + } { 1 + } if-else print 1 true { 2 + } { 1 + } if-else print }";
var input = "main () -> () { 5 { dup 0 > } { dup print 1 - } while drop }";

var parser = new Parser("input.st", input);
var program = parser.Parse();

var typedProgram = TypeInferer.Infer(program);

var evaluator = new Evaluator(program);
evaluator.Run();

var compiler = new Compiler(program);
compiler.Compile();