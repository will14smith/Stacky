using FluentAssertions;
using Xunit;

namespace Stacky.Compilation.Garbage;

public class RootTests : IDisposable
{
    private readonly RootRef _root;

    public RootTests()
    {
        _root = Root.New();
    }
    public void Dispose()
    {
        _root.Destroy();
    }
    
    [Fact]
    public void Empty_ShouldBeIterable()
    {
        _root.Should().BeEmpty();
    }
    
    [Fact]
    public void StoringValues_ShouldBeIterable()
    {
        var items = new[] { new IntPtr(1), new IntPtr(2), new IntPtr(3) };
        foreach (var item in items) { _root.Add(item); }        
        
        _root.Should().BeEquivalentTo(items);
    }  
    [Fact]
    public void StoringSameValues_ShouldBeIterable_ButNotHaveDuplicates()
    {
        var items = new[] { new IntPtr(1), new IntPtr(2), new IntPtr(3), new IntPtr(2), new IntPtr(3) };
        foreach (var item in items) { _root.Add(item); }        
        
        _root.Should().BeEquivalentTo(items.Distinct());
    }
    [Fact]
    public void StoringManyValues_ShouldBeIterable()
    {
        var items = Enumerable.Range(1, 1000).Select(x => new IntPtr(x)).ToHashSet();
        foreach (var item in items) { _root.Add(item); }

        // fluent assertions is really slow here, use custom assertions instead
        var itemsInRoot = new HashSet<IntPtr>();
        foreach (var item in _root)
        {
            Assert.True(itemsInRoot.Add(item), $"{item} is a duplicate");
            Assert.True(items.Remove(item), $"{item} was not in input");
        }

        Assert.Empty(items);
    }
    
    [Fact]
    public void RemovingDuplicateValue_ShouldDecreaseCountButNotRemove()
    {
        var items = new[] { new IntPtr(1), new IntPtr(2), new IntPtr(3), new IntPtr(2), new IntPtr(3) };
        foreach (var item in items) { _root.Add(item); }        
        
        _root.Remove(new IntPtr(2));
        
        _root.Should().BeEquivalentTo(new [] { new IntPtr(1), new IntPtr(2), new IntPtr(3) });
    }  
    [Fact]
    public void RemovingAllDuplicateValue_ShouldRemoveValue()
    {
        var items = new[] { new IntPtr(1), new IntPtr(2), new IntPtr(3), new IntPtr(2), new IntPtr(3) };
        foreach (var item in items) { _root.Add(item); }        
        
        _root.Remove(new IntPtr(2));
        _root.Remove(new IntPtr(2));
        
        _root.Should().BeEquivalentTo(new [] { new IntPtr(1), new IntPtr(3) });
    }
}