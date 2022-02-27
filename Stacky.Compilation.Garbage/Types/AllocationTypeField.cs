using System.Runtime.InteropServices;

namespace Stacky.Compilation.Garbage.Types;

[StructLayout(LayoutKind.Sequential)]
public struct AllocationTypeField
{
    [MarshalAs(UnmanagedType.LPStr)]
    public string Name;

    internal IntPtr TypePointer;
    
    public AllocationType Type
    {
        get => ReadType();
        set => WriteType(value);
    }

    private AllocationType ReadType()
    {
        if (TypePointer == IntPtr.Zero)
        {
            return default;
        }

        return Marshal.PtrToStructure<AllocationType>(TypePointer);
    }

    private void WriteType(AllocationType value)
    {
        if (TypePointer != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(TypePointer);
        }

        var type = Marshal.AllocHGlobal(Marshal.SizeOf<AllocationType>());
        Marshal.StructureToPtr(value, type, false);        
        TypePointer = type;
    }

    public static AllocationTypeField New(string name, AllocationType type) => new() { Name = name, Type = type };
}