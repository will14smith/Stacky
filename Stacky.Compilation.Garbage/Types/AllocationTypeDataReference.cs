using System.Runtime.InteropServices;

namespace Stacky.Compilation.Garbage.Types;

[StructLayout(LayoutKind.Sequential)]
public struct AllocationTypeDataReference
{
    internal IntPtr FieldsPointer;

    public AllocationTypeField[] Fields
    {
        get => ReadFields();
        set => WriteFields(value);
    }

    private unsafe AllocationTypeField[] ReadFields()
    {
        if (FieldsPointer == IntPtr.Zero)
        {
            return Array.Empty<AllocationTypeField>();
        }
        
        var list = new List<AllocationTypeField>();

        var arrayPtr = (IntPtr*) FieldsPointer;
        while (true)
        {
            var itemPtr = *arrayPtr++;
            if (itemPtr == IntPtr.Zero)
            {
                break;
            }

            var item = Marshal.PtrToStructure<AllocationTypeField>(itemPtr);
            list.Add(item);
        }
        
        return list.ToArray();
    }

    private unsafe void WriteFields(AllocationTypeField[] value)
    {
        if (FieldsPointer != IntPtr.Zero)
        {
            var oldArrayPtr = (IntPtr*) FieldsPointer;
            while (true)
            {
                var itemPtr = *oldArrayPtr++;
                if (itemPtr == IntPtr.Zero)
                {
                    break;
                }
                
                Marshal.FreeHGlobal(itemPtr);
            }

            Marshal.FreeHGlobal(FieldsPointer);
        }

        var fieldsPtr = (IntPtr*) Marshal.AllocHGlobal((value.Length + 1) * IntPtr.Size);

        var arrayPtr = fieldsPtr;
        foreach (var field in value)
        {
            var itemPtr = Marshal.AllocHGlobal(Marshal.SizeOf<AllocationTypeField>());
            Marshal.StructureToPtr(field, itemPtr, false);
            *arrayPtr++ = itemPtr;
        }
        *arrayPtr = IntPtr.Zero;
        
        FieldsPointer = (IntPtr) fieldsPtr;
    }
}