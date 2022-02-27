using System.Runtime.InteropServices;

namespace Stacky.Compilation.Garbage.Types;

[StructLayout(LayoutKind.Sequential)]
public struct AllocationTypeDataPrimitive
{
    public ulong Size;
}