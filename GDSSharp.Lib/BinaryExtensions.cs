using System.Reflection;
using System.Runtime.InteropServices;

namespace GdsSharp.Lib;

public static class BinaryExtensions
{
    public static void WriteStruct<T>(this BinaryWriter writer, T value) where T : struct
    {
        writer.Write(value.ToByteArray());
    }
    
    public static T ReadStruct<T>(this BinaryReader reader) where T : struct
    {
        var instance = new T();
        return ToStructure<T>(reader.ReadBytes(Marshal.SizeOf(instance)));
    }
    
    private static T ToStructure<T>(byte[] data) where T : struct
    {
        unsafe
        {
            fixed (byte* p = &data[0])
            {
                var val = Marshal.PtrToStructure(new IntPtr(p), typeof(T));
                if (val == null) throw new NullReferenceException("Failed to convert to structure.");
                
                // Loop over fields in val
                foreach (var prop in val.GetType().GetProperties())
                {
                    // Get value of field in val
                    var value = prop.GetValue(val);
                    if (value == null) throw new NullReferenceException("Failed to get value of field.");
                    
                    // Get field in instance
                    var instanceProp = typeof(T).GetField(prop.Name);
                    if (instanceProp == null) throw new NullReferenceException("Failed to get field in instance.");
                    
                    // Set value of field in instance
                    instanceProp.SetValue(instanceProp, value);
                }
                
                return (T) val;
            }
        }
    }
    
    private static byte[] ToByteArray<T>(this T obj) where T : struct
    {
        var len = Marshal.SizeOf(obj);
        var arr = new byte[len];
        var ptr = Marshal.AllocHGlobal(len);
        Marshal.StructureToPtr(obj, ptr, true);
        Marshal.Copy(ptr, arr, 0, len);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }
}