using System.Runtime.InteropServices;

namespace Stacky.Compilation.Garbage.Types;

[StructLayout(LayoutKind.Explicit)]
public struct AllocationTypeData
{
    [FieldOffset(0)] public AllocationTypeDataPrimitive Primitive;
    [FieldOffset(0)] public AllocationTypeDataReference Reference;
}