using Stacky;
using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Intrinsics;
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
// var input = "main () -> () { 5 { dup 0 > } { dup print 1 - } while drop }";
// var input = @"
// struct IntPair { a i64 b i64 }
//
// main () -> () {
//     @IntPair 1 ~a 2 ~b sum print drop
// }
//
// sum IntPair -> IntPair i64 {
//     dup dup #a swap #b +
// }
// ";

var _file = Path.GetTempFileName(); 
var content = "hello\nworld\nline3\nline4!";
File.WriteAllText(_file, content);

// var input = $"main () -> () {{ \"{_file}\" open-read 0 {{ over is-eof not }} {{ over read-line drop 1 + }} while swap close print }}";
// var input = "main () -> () { false 0 { over not } invoke print print print }";

// var input = "main () -> () { 1 test 2 test print print print print }\ntest i64 -> i64 str { { dup 1 + string } invoke }";

var input = "struct Test { a str }\ntest () -> Test { @Test 1 ~a }";

var parser = new Parser("input.st", input);
var program = parser.Parse();

var inferer = new TypeInferer();
All.Populate(inferer.Intrinsics);
var typedProgram = inferer.Infer(program);
TypedProgramPrinter.Print(typedProgram);

var evaluator = new Evaluator(program);
All.Populate(evaluator.Intrinsics);
evaluator.Run();

var compiler = new Compiler(typedProgram);
All.Populate(compiler.Intrinsics);
compiler.Compile();