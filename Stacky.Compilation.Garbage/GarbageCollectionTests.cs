using System.Runtime.InteropServices;
using FluentAssertions;
using Stacky.Compilation.Garbage.Types;
using Xunit;

namespace Stacky.Compilation.Garbage;

public class GarbageCollectionTests : IDisposable
{
    private readonly AllocationType _i64;
    private readonly AllocationType _type1;
    private readonly AllocationType _type2;

    private readonly GarbageCollectionRef _context;

    public GarbageCollectionTests()
    {
        _i64 = AllocationType.Primitive("i64", 8);
        
        _type1 = AllocationType.Reference("myStruct", new[]
        {
            AllocationTypeField.New("field1", _i64)
        });
        
        _type2 = AllocationType.Reference("myCoolStruct", new[]
        {
            AllocationTypeField.New("field1", _i64),
            AllocationTypeField.New("field2", _type1),
            AllocationTypeField.New("field3", _type1),
        });

        _context = GarbageCollection.New();
    }

    public void Dispose()
    {
        _context.Destroy();
    }
    
    [Fact]
    public void CanAllocateRaw()
    {
        _ = _context.AllocateRaw(100);
    }    
    
    [Fact]
    public void CanAllocate()
    {
        _ = _context.Allocate(_type2);
    }
    
    [Fact]
    public void CanRootAdd()
    {
        var data = _context.AllocateRaw(100);
        
        _context.RootAdd(data);
    }    

    [Fact]
    public void CanRootRemove()
    {
        var data = _context.AllocateRaw(100);
        _context.RootAdd(data);
        
        _context.RootRemove(data);
    }    

    
    [Fact]
    public void CanCollect()
    {
        WriteExampleData();

        var statsBefore = _context.Stats();
        _context.Collect();
        var statsAfter = _context.Stats();

        statsBefore.Should().Be(new GarbageCollectionStats
        {
            AllocatedItems = 4,
            AllocatedItemsSize = 8*2 + 24*2,
            RootedItems = 1,
            ReachableItems = 2
        });
        statsAfter.Should().Be(new GarbageCollectionStats
        {
            AllocatedItems = 2,
            AllocatedItemsSize = 8 + 24,
            RootedItems = 1,
            ReachableItems = 2
        });
    }
    
    [Fact]
    public void CanGetStats()
    {
        WriteExampleData();

        var stats = _context.Stats();

        stats.Should().Be(new GarbageCollectionStats
        {
            AllocatedItems = 4,
            AllocatedItemsSize = 8*2 + 24*2,
            RootedItems = 1,
            ReachableItems = 2
        });
    }
    
    private void WriteExampleData()
    {
        // root = [obj1], obj1 -> obj2, obj3 & obj4 are collectable
        
        var data1 = _context.Allocate(_type2);
        _context.RootAdd(data1);
        var data2 = _context.Allocate(_type1);
        Marshal.WriteIntPtr(data1.Pointer + 8, data2.Pointer);
        _ = _context.Allocate(_type1);
        _ = _context.Allocate(_type2);
    }
}