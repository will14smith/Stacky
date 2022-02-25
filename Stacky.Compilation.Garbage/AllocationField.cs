using System.Runtime.InteropServices;

namespace Stacky.Compilation.Garbage;

[StructLayout(LayoutKind.Sequential)]
public struct AllocationField
{
    [MarshalAs(UnmanagedType.LPStr)]
    public string Name;
    public AllocationKind Kind;
    internal IntPtr TypePtr;

    public AllocationType Type
    {
        get => Marshal.PtrToStructure<AllocationType>(TypePtr);
        set
        {
            // TODO free existing
            TypePtr = Marshal.AllocHGlobal(Marshal.SizeOf<AllocationField>());
            Marshal.StructureToPtr(value, TypePtr, false);
        }
    }
}