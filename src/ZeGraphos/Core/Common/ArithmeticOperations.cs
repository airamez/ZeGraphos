using System;

namespace ZeGraphos.Core.Common;

/// <summary>
/// Provides arithmetic operations for generic numeric types.
/// Used by algorithms that need to work with any numeric type.
/// </summary>
/// <typeparam name="T">The numeric type</typeparam>
public static class ArithmeticOperations<T>
    where T : struct, IComparable<T>, IConvertible, IEquatable<T>
{
    /// <summary>
    /// Adds two numeric values.
    /// </summary>
    public static T Add(T a, T b)
    {
        if (typeof(T) == typeof(double)) return (T)(object)((double)(object)a + (double)(object)b);
        if (typeof(T) == typeof(int)) return (T)(object)((int)(object)a + (int)(object)b);
        if (typeof(T) == typeof(float)) return (T)(object)((float)(object)a + (float)(object)b);
        if (typeof(T) == typeof(decimal)) return (T)(object)((decimal)(object)a + (decimal)(object)b);
        if (typeof(T) == typeof(long)) return (T)(object)((long)(object)a + (long)(object)b);
        if (typeof(T) == typeof(short)) return (T)(object)((short)(object)a + (short)(object)b);
        if (typeof(T) == typeof(uint)) return (T)(object)((uint)(object)a + (uint)(object)b);
        if (typeof(T) == typeof(ulong)) return (T)(object)((ulong)(object)a + (ulong)(object)b);
        if (typeof(T) == typeof(ushort)) return (T)(object)((ushort)(object)a + (ushort)(object)b);
        
        throw new NotSupportedException($"Addition not supported for type {typeof(T)}");
    }

    /// <summary>
    /// Compares two numeric values.
    /// </summary>
    public static int Compare(T a, T b)
    {
        if (typeof(T) == typeof(double)) return ((double)(object)a).CompareTo((double)(object)b);
        if (typeof(T) == typeof(int)) return ((int)(object)a).CompareTo((int)(object)b);
        if (typeof(T) == typeof(float)) return ((float)(object)a).CompareTo((float)(object)b);
        if (typeof(T) == typeof(decimal)) return ((decimal)(object)a).CompareTo((decimal)(object)b);
        if (typeof(T) == typeof(long)) return ((long)(object)a).CompareTo((long)(object)b);
        if (typeof(T) == typeof(short)) return ((short)(object)a).CompareTo((short)(object)b);
        if (typeof(T) == typeof(uint)) return ((uint)(object)a).CompareTo((uint)(object)b);
        if (typeof(T) == typeof(ulong)) return ((ulong)(object)a).CompareTo((ulong)(object)b);
        if (typeof(T) == typeof(ushort)) return ((ushort)(object)a).CompareTo((ushort)(object)b);
        
        return a.CompareTo(b);
    }

    /// <summary>
    /// Gets the maximum value for the type.
    /// </summary>
    public static T GetMaxValue()
    {
        if (typeof(T) == typeof(double)) return (T)(object)double.MaxValue;
        if (typeof(T) == typeof(int)) return (T)(object)int.MaxValue;
        if (typeof(T) == typeof(float)) return (T)(object)float.MaxValue;
        if (typeof(T) == typeof(decimal)) return (T)(object)decimal.MaxValue;
        if (typeof(T) == typeof(long)) return (T)(object)long.MaxValue;
        if (typeof(T) == typeof(short)) return (T)(object)short.MaxValue;
        if (typeof(T) == typeof(uint)) return (T)(object)uint.MaxValue;
        if (typeof(T) == typeof(ulong)) return (T)(object)ulong.MaxValue;
        if (typeof(T) == typeof(ushort)) return (T)(object)ushort.MaxValue;
        
        // For other types, try to use dynamic
        try
        {
            dynamic maxValue = default(T);
            if (typeof(T).IsEnum)
            {
                var values = Enum.GetValues(typeof(T));
                maxValue = values.GetValue(values.Length - 1) ?? maxValue;
            }
            return maxValue;
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Gets the minimum value for the type.
    /// </summary>
    public static T GetMinValue()
    {
        if (typeof(T) == typeof(double)) return (T)(object)double.MinValue;
        if (typeof(T) == typeof(int)) return (T)(object)int.MinValue;
        if (typeof(T) == typeof(float)) return (T)(object)float.MinValue;
        if (typeof(T) == typeof(decimal)) return (T)(object)decimal.MinValue;
        if (typeof(T) == typeof(long)) return (T)(object)long.MinValue;
        if (typeof(T) == typeof(short)) return (T)(object)short.MinValue;
        if (typeof(T) == typeof(uint)) return (T)(object)uint.MinValue;
        if (typeof(T) == typeof(ulong)) return (T)(object)ulong.MinValue;
        if (typeof(T) == typeof(ushort)) return (T)(object)ushort.MinValue;
        
        // For other types, try to use dynamic
        try
        {
            dynamic minValue = default(T);
            if (typeof(T).IsEnum)
            {
                var values = Enum.GetValues(typeof(T));
                minValue = values.GetValue(0) ?? minValue;
            }
            return minValue;
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Gets the zero value for the type.
    /// </summary>
    public static T Zero
    {
        get
        {
            if (typeof(T) == typeof(double)) return (T)(object)0.0;
            if (typeof(T) == typeof(int)) return (T)(object)0;
            if (typeof(T) == typeof(float)) return (T)(object)0.0f;
            if (typeof(T) == typeof(decimal)) return (T)(object)0m;
            if (typeof(T) == typeof(long)) return (T)(object)0L;
            if (typeof(T) == typeof(short)) return (T)(object)0;
            if (typeof(T) == typeof(uint)) return (T)(object)0u;
            if (typeof(T) == typeof(ulong)) return (T)(object)0ul;
            if (typeof(T) == typeof(ushort)) return (T)(object)0;
            
            return default(T);
        }
    }
}
