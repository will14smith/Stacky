using Xunit;

namespace Stacky.Compilation.Garbage;

public class Tests
{
    [Fact]
    public void CanCreateAndDestroyContext()
    {
        var context = GarbageCollection.New();
        context.Destroy();
    } 
    
    [Fact]
    public void CanAllocateRaw()
    {
        var context = GarbageCollection.New();
        _ = context.AllocateRaw(100);
        context.Destroy();
    }    
    
    [Fact]
    public void CanAllocate()
    {
        var type1 = new AllocationType
        {
            Name = "myStruct",
            Fields = new AllocationField[]
            {
                new() { Name = "field1", Kind = AllocationKind.Primitive },
            }
        };
        
        var type2 = new AllocationType
        {
            Name = "myCoolStruct",
            Fields = new AllocationField[]
            {
                new() { Name = "field1", Kind = AllocationKind.Primitive },
                new() { Name = "field2", Kind = AllocationKind.Reference, Type = type1 },
            }
        };
        
        var context = GarbageCollection.New();
        _ = context.Allocate(type2);
        context.Destroy();
    }
    
    [Fact]
    public void CanRootAdd()
    {
        var context = GarbageCollection.New();
        var data = context.AllocateRaw(100);
        context.RootAdd(data);
        context.Destroy();
    }    

    [Fact]
    public void CanRootRemove()
    {
        var context = GarbageCollection.New();
        var data = context.AllocateRaw(100);
        context.RootAdd(data);
        context.RootRemove(data);
        context.Destroy();
    }    

}