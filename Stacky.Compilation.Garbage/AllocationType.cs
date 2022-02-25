using System.Runtime.InteropServices;

namespace Stacky.Compilation.Garbage;

[StructLayout(LayoutKind.Sequential)]
public struct AllocationType
{
    [MarshalAs(UnmanagedType.LPStr)]
    public string Name;
    internal int NumFields;
    internal IntPtr FieldsPtr;

    public unsafe AllocationField[] Fields
    {
        get
        {
            var fields = new AllocationField[NumFields];

            var ptr = FieldsPtr;
            for (var i = 0; i < fields.Length; i++)
            {
                fields[i] = Marshal.PtrToStructure<AllocationField>(ptr);
                ptr += sizeof(IntPtr);
            }
            
            return fields;
        }
        set
        {
            // TODO should free fields array & fields in array
            
            NumFields = value.Length;
            FieldsPtr = Marshal.AllocHGlobal(NumFields * sizeof(IntPtr));

            var ptr = (IntPtr*)FieldsPtr;
            for (var i = 0; i < value.Length; i++)
            {
                var field = Marshal.AllocHGlobal(Marshal.SizeOf<AllocationField>());
                Marshal.StructureToPtr(value[i], field, false);

                *ptr++ = field;
            }
        }
    }
}