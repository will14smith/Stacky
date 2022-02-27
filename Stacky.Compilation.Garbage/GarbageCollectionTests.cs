using Stacky.Compilation.Garbage.Types;
using Xunit;

namespace Stacky.Compilation.Garbage;

public class GarbageCollectionTests
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
        var i64 = AllocationType.Primitive("i64", 8);
        var type1 = AllocationType.Reference("myStruct", new[]
        {
            AllocationTypeField.New("field1", i64)
        });
        var type2 = AllocationType.Reference("myCoolStruct", new[]
        {
            AllocationTypeField.New("field1", i64),
            AllocationTypeField.New("field2", type1),
        });
        
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