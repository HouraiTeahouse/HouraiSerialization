using System.Runtime.InteropServices;
using System.Text;

namespace HouraiTeahouse.Serialization {

public static class SerializationConstants {

  public static readonly Encoding Encoding = new UTF8Encoding();
  public static int kMaxMessageSize = (int)ushort.MaxValue;

}

public static class ZigZag {

  /// <summary>
  /// Encodes an integer value into a ZigZag encoded unsigned 
  /// integer with a specified bit length.
  /// </summary>
  /// <param name="value">the integer value to encode.</param>
  /// <param name="bitLength">the length of the integer in bits.</param>
  /// <returns>the encoded unsigned integer</returns>
  public static ulong Encode(long value, int bitLength) {
    unchecked {
      return (ulong)((value << 1) ^ (value >> (bitLength - 1)));
    }
  }

  /// <summary>
  /// Decodes an unsigned ZigZag encoded integer back into
  /// an signed integer.
  /// </summary>
  /// <param name="value">the ZigZag encoded unsigned integer</param>
  /// <returns>the decoded signed integer</returns>
  public static long Decode(ulong value) {
    unchecked {
      if ((value & 0x1) == 0x1) {
        return -1 * ((long)(value >> 1) + 1);
      }
      return (long)(value >> 1);
    }
  }

}

// -- helpers for float conversion --
// This cannot be used with IL2CPP because it cannot convert FieldOffset at the moment
// Until that is supported the IL2CPP codepath will use BitConverter instead of this. Use
// of BitConverter is otherwise not optimal as it allocates a byte array for each conversion.
#if !INCLUDE_IL2CPP
[StructLayout(LayoutKind.Explicit)]
internal struct UIntFloat {
    [FieldOffset(0)]
    public float floatValue;

    [FieldOffset(0)]
    public uint intValue;

    [FieldOffset(0)]
    public double doubleValue;

    [FieldOffset(0)]
    public ulong longValue;
}

internal class FloatConversion {

  public static float ToSingle(uint value) {
    UIntFloat uf = new UIntFloat();
    uf.intValue = value;
    return uf.floatValue;
  }

  public static double ToDouble(ulong value) {
    UIntFloat uf = new UIntFloat();
    uf.longValue = value;
    return uf.doubleValue;
  }

}
#endif // !INCLUDE_IL2CPP

}