using System;
using System.IO;
using FluentAssertions;
using Stacky.Evaluation;
using Xunit;

namespace Stacky.Tests;

public class IoSemantics : SemanticsBase, IDisposable
{
    // TODO can the evaluator be hooked to use a virtual fs? 

    // open-read     string -> File
    // open-write    string -> File
    // open-append   string -> File
    // close         File -> ()
    
    // read-u8       File -> u8
    // read-str      File -> str
    // read-line     File -> str
    
    // is-eof        File -> bool
    
    // write-u8      File u8 -> ()
    // write-str File str -> ()
    // write-line File str -> ()
    
    private readonly string _file;

    public IoSemantics()
    {
        _file = Path.GetTempFileName();
    }
    
    public void Dispose()
    {
        if (File.Exists(_file))
        {
            File.Delete(_file);
        }
    }
    
    [Fact]
    public void FilesCanBeOpenedAndClosed()
    {
        var run = () => RunExpr($"\"{_file}\" open-read close");

        run.Should().NotThrow();
    }
    
    [Fact]
    public void FilesCanBeCreated()
    {
        File.Delete(_file);

        RunExpr($"\"{_file}\" open-write close");

        File.Exists(_file).Should().BeTrue();
    }
    
    [Fact]
    public void FilesCanBeOverwritten()
    {
        File.WriteAllText(_file, "hello world");
        
        RunExpr($"\"{_file}\" open-write close");

        File.Exists(_file).Should().BeTrue();
        File.ReadAllText(_file).Should().BeEmpty();
    }

    [Fact]
    public void FilesCanBeAppended()
    {
        var content = "hello world";
        File.WriteAllText(_file, content);
        
        RunExpr($"\"{_file}\" open-append close");

        File.Exists(_file).Should().BeTrue();
        File.ReadAllText(_file).Should().Be(content);
    }

    [Fact] 
    public void FilesCanBeReadByteByByte()
    {
        var content = "hello world";
        File.WriteAllText(_file, content);
        
        var values = RunExpr($"\"{_file}\" open-read dup read-u8 swap close");

        values.Should().HaveCount(1);
        values[0].Should().BeOfType<EvaluationValue.Int64>().Which.Value.Should().Be((byte)'h');
    }

    [Fact]
    public void FilesCanBeReadAsString()
    {
        var content = "hello world";
        File.WriteAllText(_file, content);
        
        var values = RunExpr($"\"{_file}\" open-read dup read-str swap close");

        values.Should().HaveCount(1);
        values[0].Should().BeOfType<EvaluationValue.String>().Which.Value.Should().Be(content);
    }
    
    [Fact]
    public void FilesCanBeReadLineByLine()
    {
        var content = "hello\nworld";
        File.WriteAllText(_file, content);
        
        var values = RunExpr($"\"{_file}\" open-read dup read-line swap close");

        values.Should().HaveCount(1);
        values[0].Should().BeOfType<EvaluationValue.String>().Which.Value.Should().Be("hello");
    }
    
    [Fact]
    public void FilesCanBeReadLineByLineToEnd()
    {
        var content = "hello\nworld\nline3\nline4!";
        File.WriteAllText(_file, content);
        
        var values = RunExpr($"\"{_file}\" open-read 0 {{ over is-eof not }} {{ over read-line drop 1 + }} while swap drop");

        values.Should().HaveCount(1);
        values[0].Should().BeOfType<EvaluationValue.Int64>().Which.Value.Should().Be(4);
    }


    [Fact]
    public void FilesCanBeWrittenByteByByte()
    {
        var content = "h";
        
        RunExpr($"\"{_file}\" open-write dup {(byte)content[0]} write-u8 close");

        File.ReadAllText(_file).Should().Be(content);
    }

    [Fact]
    public void FilesCanBeWrittenAsString()
    {
        var content = "hello world";
        
        RunExpr($"\"{_file}\" open-write dup \"{content}\" write-str close");

        File.ReadAllText(_file).Should().Be(content);
    }

    [Fact]
    public void FilesCanBeWrittenLineByLine()
    {
        var content = "hello\nworld!";
        var lines = content.Split('\n'); 

        RunExpr($"\"{_file}\" open-write dup \"{lines[0]}\" write-line dup \"{lines[1]}\" write-str close");

        File.ReadAllText(_file).Should().Be(content);
    }
}