using System.Diagnostics;
using System.Text;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace Tests;

public class Runner
{
    private readonly string _projectFile = Path.GetFullPath("../Stacky/Stacky.csproj");
    private readonly string _exeFile = Path.GetFullPath("../Stacky/bin/Debug/net6.0/Stacky");
    private readonly string _gcFile = Path.GetFullPath("../Stacky.Compilation/nullgc.c");
    
    public bool OutputTimes { get; set; }
    public bool SkipBuild { get; set; }
    
    public IEnumerable<string> GetAllTests() => Directory.GetFiles(".", "*.st").Select(Path.GetFullPath);

    public void RunAll()
    {
        foreach (var file in GetAllTests())
        {
            Run(file);
        }
    }

    public async Task RunAllAsync()
    {
        var tasks = GetAllTests().Select(RunAsync);

        await Task.WhenAll(tasks);
    }

    public void Run(string file)
    {
        Console.WriteLine($"=== {Path.GetFileNameWithoutExtension(file)} ===");

        var evalOutput = RunEvaluate(file);
        Compare("Evaluated", evalOutput, file);

        var compOutput = RunCompile(file);
        Compare("Compiled", compOutput, file);
    }

    private readonly SemaphoreSlim _consoleLock = new (1);
    public async Task RunAsync(string file)
    {
        var evalOutput = Task.Run(() => RunEvaluate(file));
        var compOutput = Task.Run(() => RunCompile(file));

        await _consoleLock.WaitAsync();
        try
        {
            Console.WriteLine($"=== {Path.GetFileNameWithoutExtension(file)} ===");
            Compare("Evaluated", await evalOutput, file);
            Compare("Compiled", await compOutput, file);
        }
        finally
        {
            _consoleLock.Release();
        }
    }
    
    public void Update(string file)
    {
        Console.WriteLine($"=== {Path.GetFileNameWithoutExtension(file)} ===");

        var output = RunEvaluate(file);
        File.WriteAllText(PathToGiven(file), output.Format());
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"✏ Updated {Path.GetFileNameWithoutExtension(file)}");
        Console.ResetColor();
    }

    private void Compare(string name, Output output, string file)
    {
        var givenFile = PathToGiven(file);
        
        if (!File.Exists(givenFile))
        {
            if (!output.WasSuccessful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✏ Update failed {Path.GetFileNameWithoutExtension(file)}");
                Console.ResetColor();

            }
            
            File.WriteAllText(PathToGiven(file), output.Format());
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"✏ Updated {Path.GetFileNameWithoutExtension(file)}");
            Console.ResetColor();

            return;
        }

        var given = File.ReadAllText(givenFile);

        var diff = InlineDiffBuilder.Diff(given, output.Format());
        if (!diff.HasDifferences)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✔ {name} {Path.GetFileNameWithoutExtension(file)}");
            Console.ResetColor();
            
            return;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ {name} {Path.GetFileNameWithoutExtension(file)}");
        Console.ResetColor();

        foreach (var line in diff.Lines)
        {
            switch (line.Type)
            {
                case ChangeType.Inserted:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("  + ");
                    break;
                case ChangeType.Deleted:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("  - ");
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Gray; // compromise for dark or light background
                    Console.Write("    ");
                    break;
            }

            Console.WriteLine(line.Text);
        }
        Console.ResetColor();
    }

    private string PathToGiven(string file) => Path.Join(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + ".out");
    
    public Output RunEvaluate(string file)
    {
        var tempDir = Path.GetTempFileName();
        File.Delete(tempDir);
        Directory.CreateDirectory(tempDir);
        
        var timer = new Stopwatch();
        timer.Start();
        try
        {
            var args = $"evaluate --file \"{file}\"";
            var output = SkipBuild 
                ? Exec(_exeFile, args, tempDir) 
                : Exec("dotnet", $"run --project \"{_projectFile}\" -- {args}", tempDir);
            if (output.WasSuccessful)
            {
                SkipBuild = true;
            }

            if (OutputTimes)
            {
                Console.WriteLine($"Evaluation took {timer.Elapsed}");
            }
            
            return output;
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
    public Output RunCompile(string file)
    {
        var tempDir = Path.GetTempFileName();
        File.Delete(tempDir);
        Directory.CreateDirectory(tempDir);

        var objectFile = Path.Combine(tempDir, "a.o");

        var timer = new Stopwatch();
        timer.Start();
        try
        {
            var args = $"compile --file \"{file}\" --output \"{objectFile}\"";
            var compileOutput = SkipBuild 
                ? Exec(_exeFile, args, tempDir) 
                : Exec("dotnet", $"run --project \"{_projectFile}\" -- {args}", tempDir);

            if (compileOutput.Exit != 0)
            {
                return compileOutput;
            }
            SkipBuild = true;
        
            var gccOutput = Exec("gcc", $"\"{objectFile}\" \"{_gcFile}\"", tempDir);
            if (!gccOutput.WasSuccessful)
            {
                return compileOutput.Combine(gccOutput);
            }

            if (OutputTimes)
            {
                Console.WriteLine($"Compiling took {timer.Elapsed}");
            }

            timer.Restart();

            var output = Exec(Path.Combine(tempDir, "a.out"), "", tempDir);

            if (OutputTimes)
            {
                Console.WriteLine($"Running took {timer.Elapsed}");
            }

            return compileOutput.Combine(output);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
    
    private static Output Exec(string file, string args, string dir)
    {
        var process = new Process();

        process.StartInfo.FileName = file;
        process.StartInfo.Arguments = args;
        process.StartInfo.WorkingDirectory = dir;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
    
        process.Start();

        process.WaitForExit();

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
    
        return new Output(output, error, process.ExitCode);
    }
}

public record Output(string Out, string Err, int Exit)
{
    public bool WasSuccessful => string.IsNullOrEmpty(Err) && Exit == 0;
    
    public string Format()
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== Output ===");
        sb.AppendLine(Out);

        if (!string.IsNullOrEmpty(Err))
        {
            sb.AppendLine("=== Error ===");
            sb.AppendLine(Err);
        }

        sb.AppendLine("=== Exit ===");
        sb.AppendLine($"{Exit}");
        
        return sb.ToString();
    }

    public Output Combine(Output other) => new(Out + other.Out, Err + other.Err, other.Exit);
}