using System.Runtime.InteropServices;

namespace Stacky.Compilation.Garbage.Types;

[StructLayout(LayoutKind.Sequential)]
public struct AllocationType
{
    [MarshalAs(UnmanagedType.LPStr)]
    public string Name;
    
    public AllocationTypeKind Kind;
    public AllocationTypeData Data;

    public static AllocationType Primitive(string name, ulong size) => new()
    {
        Name = name,
        Kind = AllocationTypeKind.Primitive,
        Data = { Primitive = { Size = size } }
    }; 
    
    public static AllocationType Reference(string name, AllocationTypeField[] fields) => new()
    {
        Name = name,
        Kind = AllocationTypeKind.Reference,
        Data = { Reference = { Fields = fields } }
    };
}