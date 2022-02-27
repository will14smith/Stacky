using FluentAssertions;
using Xunit;

namespace Stacky.Compilation.Garbage;

public class HeapTests : IDisposable
{
    private readonly HeapRef _heap;

    public HeapTests()
    {
        _heap = Heap.New();
    }
    public void Dispose()
    {
        _heap.Destroy();
    }
    
    [Fact]
    public void Empty_ShouldHaveCount()
    {
        _heap.Count.Should().Be(0);
    } 
    [Fact]
    public void Empty_ShouldBeIterable()
    {
        _heap.Should().BeEmpty();
    }

    [Fact]
    public void StoringValues_ShouldHaveCount()
    {
        var items = new[] { new IntPtr(1), new IntPtr(2), new IntPtr(3) };
        foreach (var item in items) { _heap.Add(item); }        
        
        _heap.Count.Should().Be(3);
    }
    [Fact]
    public void StoringValues_ShouldBeIterable()
    {
        var items = new[] { new IntPtr(1), new IntPtr(2), new IntPtr(3) };
        foreach (var item in items) { _heap.Add(item); }        
        
        _heap.Should().BeEquivalentTo(items);
    }
    [Fact]
    public void StoringManyValues_ShouldBeIterable()
    {
        var items = Enumerable.Range(1, 1000).Select(x => new IntPtr(x)).ToArray();
        foreach (var item in items) { _heap.Add(item); }
        
        _heap.Should().BeEquivalentTo(items);
    }
    
    [Fact]
    public void RemovingValues_ShouldRemoveAll()
    {
        var items = new[] { new IntPtr(1), new IntPtr(2), new IntPtr(3), new IntPtr(2), new IntPtr(4) };
        foreach (var item in items) { _heap.Add(item); }        
        
        _heap.Remove(new IntPtr(2));
        
        _heap.Count.Should().Be(3);
        _heap.Should().BeEquivalentTo(new [] { new IntPtr(1), new IntPtr(3), new IntPtr(4) });
    }
}