using System.CommandLine;
using Stacky;
using Stacky.Compilation;
using Stacky.Evaluation;
using Stacky.Intrinsics;
using Stacky.Parsing;
using Stacky.Parsing.Typing;

var fileOption = new Option<FileInfo>("--file");
fileOption.IsRequired = true;

var outputOption = new Option<FileInfo>("--output");
outputOption.IsRequired = true;

var typeOption = new Option<bool>("--type");

var evaluateCommand = new Command("evaluate") { fileOption, typeOption };
evaluateCommand.AddAlias("eval");
evaluateCommand.SetHandler((FileInfo file, bool type) =>
{
    var input = File.ReadAllText(file.FullName);
    var parser = new Parser(file.FullName, input);
    var program = parser.Parse();

    var inferer = new TypeInferer();
    All.Populate(inferer.Intrinsics);
    var typedProgram = inferer.Infer(program);
    if (type)
    {
        TypedProgramPrinter.Print(typedProgram);
    }

    var evaluator = new Evaluator(program);
    All.Populate(evaluator.Intrinsics);
    evaluator.Run();
}, fileOption, typeOption);

var compileCommand = new Command("compile") { fileOption, outputOption };
compileCommand.SetHandler((FileInfo file, FileInfo output) =>
{
    var input = File.ReadAllText(file.FullName);
    var parser = new Parser(file.FullName, input);
    var program = parser.Parse();

    var inferer = new TypeInferer();
    All.Populate(inferer.Intrinsics);
    var typedProgram = inferer.Infer(program);
    
    var compiler = new Compiler(typedProgram);
    All.Populate(compiler.Intrinsics);
    compiler.Compile(output.FullName);
}, fileOption, outputOption);

var rootCommand = new RootCommand
{
    evaluateCommand,
    compileCommand
};
rootCommand.Invoke(args);