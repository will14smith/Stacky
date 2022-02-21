using System.CommandLine;
using Tests;

var fileOption = new Option<FileInfo>("--file").ExistingOnly();
fileOption.IsRequired = true;

var forceBuild = new Option<bool>("--force-build");

var runCommand = new Command("run") { fileOption };
runCommand.SetHandler((FileInfo file, bool force) => Run(file, force), fileOption, forceBuild);

var updateCommand = new Command("update") { fileOption };
updateCommand.SetHandler((FileInfo file, bool force) => Update(file, force), fileOption, forceBuild);
var rootCommand = new RootCommand
{
    runCommand,
    updateCommand
};
rootCommand.AddGlobalOption(forceBuild);
rootCommand.SetHandler((bool force) => RunAll(force), forceBuild);
rootCommand.Invoke(args);

static async Task RunAll(bool force)
{
    var runner = new Runner { SkipBuild = !force };
    await runner.RunAllAsync();
}
static async Task Run(FileInfo file, bool force)
{
    var runner = new Runner { SkipBuild = !force };
    await runner.RunAsync(file.FullName);
}
static void Update(FileInfo file, bool force)
{
    var runner = new Runner { SkipBuild = !force };
    runner.Update(file.FullName);
}

