using System;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections.LowLevel.Unsafe;

namespace HouraiTeahouse.Serialization {

public interface IDeserializer : IBufferIO {
}

public static unsafe class IDeserializerExtensions {

  /// <summary>
  /// Reads a single byte from the buffer.
  /// </summary>
  public static byte ReadByte<T>(this ref T deserializer) where T : struct, IDeserializer {
    Assert.IsTrue(deserializer.IsValid());
    deserializer.Reserve(1);
    return *deserializer.Current++;
  }

  /// <summary>
  /// Reads a single signed byte from the buffer.
  /// </summary>
  public static sbyte ReadSByte<T>(this ref T deserializer) where T : struct, IDeserializer {
    return (sbyte)deserializer.ReadByte();
  }

  /// <summary>
  /// Reads a 2 byte ushort from the buffer, as a 1-3 byte varint in big endian,
  /// regarldess of platform.
  ///
  /// See: http://sqlite.org/src4/doc/trunk/www/varint.wiki.
  /// </summary>
  public static ushort ReadUInt16<T>(this ref T deserializer) where T : struct, IDeserializer {
    byte a0 = deserializer.ReadByte();
    if (a0 < 241) return a0;
    byte a1 = deserializer.ReadByte();
    if (a0 >= 241 && a0 <= 248) return (ushort)(240 + 256 * (a0 - ((ushort)241)) + a1);
    byte a2 = deserializer.ReadByte();
    if (a0 == 249) return (ushort)(2288 + (((ushort)256) * a1) + a2);
    throw new IndexOutOfRangeException("ReadPackedUInt16() failure: " + a0);
  }

  /// <summary>
  /// Reads a 4 byte uint from the buffer, as a 1-5 byte varint in big endian,
  /// regarldess of platform.
  ///
  /// See: http://sqlite.org/src4/doc/trunk/www/varint.wiki.
  /// </summary>
  public static uint ReadUInt32<T>(this ref T deserializer) where T : struct, IDeserializer {
    byte a0 = deserializer.ReadByte();
    if (a0 < 241) return a0;
    byte a1 = deserializer.ReadByte();
    if (a0 >= 241 && a0 <= 248) return (UInt32)(240 + 256 * (a0 - 241) + a1);
    byte a2 = deserializer.ReadByte();
    if (a0 == 249) return (UInt32)(2288 + 256 * a1 + a2);
    byte a3 = deserializer.ReadByte();
    if (a0 == 250) return a1 + (((UInt32)a2) << 8) + (((UInt32)a3) << 16);
    byte a4 = deserializer.ReadByte();
    if (a0 >= 251) return a1 + (((UInt32)a2) << 8) + (((UInt32)a3) << 16) + (((UInt32)a4) << 24);
    throw new IndexOutOfRangeException("ReadUInt32() failure: " + a0);
  }

  /// <summary>
  /// Reads a 8 byte ulong from the buffer, as a 1-8 byte varint in big endian,
  /// regarldess of platform.
  ///
  /// See: http://sqlite.org/src4/doc/trunk/www/varint.wiki.
  /// </summary>
  public static ulong ReadUInt64<T>(this ref T deserializer) where T : struct, IDeserializer {
    byte a0 = deserializer.ReadByte();
    if (a0 < 241) return a0;
    byte a1 = deserializer.ReadByte();
    if (a0 >= 241 && a0 <= 248) return 240 + 256 * (a0 - ((UInt64)241)) + a1;
    byte a2 = deserializer.ReadByte();
    if (a0 == 249) return 2288 + (((UInt64)256) * a1) + a2;
    byte a3 = deserializer.ReadByte();
    if (a0 == 250) return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16);
    byte a4 = deserializer.ReadByte();
    if (a0 == 251) {
      return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24);
    }
    byte a5 = deserializer.ReadByte();
    if (a0 == 252) {
      return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32);
    }
    byte a6 = deserializer.ReadByte();
    if (a0 == 253) {
      return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40);
    }
    byte a7 = deserializer.ReadByte();
    if (a0 == 254) {
      return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40) + (((UInt64)a7) << 48);
    }
    byte a8 = deserializer.ReadByte();
    if (a0 == 255) {
      return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40) + (((UInt64)a7) << 48)  + (((UInt64)a8) << 56);
    }
    throw new IndexOutOfRangeException("ReadUInt64() failure: " + a0);
  }

  /// <summary>
  /// Reads a 2 byte short from the buffer, as a 1-3 byte varint in big endian,
  /// regarldess of platform.
  ///
  /// Signed integers are encoded via zigzag encoding for more efficient encoding
  /// of negative values. See:
  /// https://developers.google.com/protocol-buffers/docs/encoding#signed-integers
  /// for more information.
  ///
  /// See: http://sqlite.org/src4/doc/trunk/www/varint.wiki
  /// </summary>
  public static short ReadInt16<T>(this ref T deserializer) where T : struct, IDeserializer
    => (short)ZigZag.Decode(deserializer.ReadUInt16());

  /// <summary>
  /// Reads a 4 byte integer from the buffer, as a 1-5 byte varint in big endian,
  /// regarldess of platform.
  ///
  /// Signed integers are encoded via zigzag encoding for more efficient encoding
  /// of negative values. See:
  /// https://developers.google.com/protocol-buffers/docs/encoding#signed-integers
  /// for more information.
  ///
  /// See: http://sqlite.org/src4/doc/trunk/www/varint.wiki
  /// </summary>
  public static int ReadInt32<T>(this ref T deserializer) where T : struct, IDeserializer 
    => (int)ZigZag.Decode(deserializer.ReadUInt32());

  /// <summary>
  /// Reads a 8 byte integer from the buffer, as a 1-9 byte varint in big endian,
  /// regarldess of platform.
  ///
  /// Signed integers are encoded via zigzag encoding for more efficient encoding
  /// of negative values. See:
  /// https://developers.google.com/protocol-buffers/docs/encoding#signed-integers
  /// for more information.
  ///
  /// See: http://sqlite.org/src4/doc/trunk/www/varint.wiki
  /// </summary>
  public static long ReadInt64<T>(this ref T deserializer) where T : struct, IDeserializer
    => (long)ZigZag.Decode(deserializer.ReadUInt64());

  /// <summary>
  /// Reads a 4 byte float from the buffer. This is always 4 bytes on the wire.
  /// </summary>
  public static float ReadSingle<T>(this ref T deserializer) where T : struct, IDeserializer {
#if INCLUDE_IL2CPP
    return BitConverter.ToSingle(BitConverter.GetBytes(deserializer.ReadUInt32()), 0);
#else
    uint value = deserializer.ReadUInt32();
    return FloatConversion.ToSingle(value);
#endif
  }

  /// <summary>
  /// Reads a 8 byte float from the buffer. This is always 8 bytes on the wire.
  /// </summary>
  public static double ReadDouble<T>(this ref T deserializer) where T : struct, IDeserializer {
#if INCLUDE_IL2CPP
    return BitConverter.ToDouble(BitConverter.GetBytes(deserializer.ReadUInt64()), 0);
#else
    ulong value = deserializer.ReadUInt64();
    return FloatConversion.ToDouble(value);
#endif
  }

  /// <summary>
  /// Reads a UTF-8 encoded string from the buffer. The maximum supported length
  /// of the encoded string is 65535 bytes.
  /// </summary>
  public static string ReadString<T>(this ref T deserializer) where T : struct, IDeserializer {
    ushort count = deserializer.ReadUInt16();
    if (count == 0) return "";
    deserializer.Reserve(count);
    var decodedString = SerializationConstants.Encoding.GetString(
      deserializer.Current, (int)count);
    deserializer.Current += count;
    return decodedString;
  }

  /// <summary>
  /// Reads a single character from the buffer.
  /// </summary>
  public static char ReadChar<T>(this ref T deserializer) where T : struct, IDeserializer 
    => (char)deserializer.ReadUInt16();

  /// <summary>
  /// Reads a boolean from the buffer. This is 1 byte on the wire. It may be
  /// preferable to write and read from a bitmask to if there are multiple
  /// boolean values to be encoded/decoded.
  /// </summary>
  public static bool ReadBoolean<T>(this ref T deserializer) where T : struct, IDeserializer 
    => deserializer.ReadByte() != 0;

  /// <summary>
  /// Reads a fixed size byte span from the buffer. 
  /// The maximum supported length of the byte span is 65535 bytes. 
  /// </summary>
  public static ReadOnlySpan<byte> ReadBytes<T>(this ref T deserializer, int count) where T : struct, IDeserializer {
    if (count < 0) {
      throw new IndexOutOfRangeException("ReadBytes " + count);
    }
    deserializer.Reserve(count);
    var span = new ReadOnlySpan<byte>(deserializer.Current, count);
    deserializer.Current += count;
    return span;
  }

  /// <summary>
  /// Reads fixed sized buffer into a provided buffer. The maximum supported
  /// length of the byte array is 65535 bytes.
  ///
  /// This function only does bounds checking on the underlying read buffer. It's
  /// upon the caller to ensure the memory being written to is safe.
  /// </summary>
  public static void ReadBytes<T>(this ref T deserializer, byte* buffer, int count) 
                                  where T : struct, IDeserializer {
    if (count < 0) {
      throw new IndexOutOfRangeException("NetworkReader ReadBytes " + count);
    }
    deserializer.Reserve(count);
    UnsafeUtility.MemCpy(buffer, deserializer.Current, count);
    deserializer.Current += count;
  }

  /// <summary>
  /// Reads fixed size byte span from the buffer using the size encoded in the
  /// underlying buffer. The maximum supported length of the byte array is 65535 bytes.
  /// </summary>
  public static ReadOnlySpan<byte> ReadBytesAndSize<T>(this ref T deserializer, int count) 
                                                       where T : struct, IDeserializer {
    ushort sz = deserializer.ReadUInt16();
    if (sz == 0) return null;
    return deserializer.ReadBytes(sz);
  }

  /// <summary>
  /// Reads a Vector2 from the underlying buffer. This will always be 8 bytes on
  /// the wire.
  /// </summary>
  public static Vector2 ReadVector2<T>(this ref T deserializer) where T : struct, IDeserializer {
    return new Vector2(deserializer.ReadSingle(), deserializer.ReadSingle());
  }

  /// <summary>
  /// Reads a Vector3 from the underlying buffer. This will always be 12 bytes on
  /// the wire.
  /// </summary>
  public static Vector3 ReadVector3<T>(this ref T deserializer) where T : struct, IDeserializer {
    return new Vector3(deserializer.ReadSingle(), deserializer.ReadSingle(), deserializer.ReadSingle());
  }

  /// <summary>
  /// Reads a Vector4 from the underlying buffer. This will always be 16 bytes on
  /// the wire.
  /// </summary>
  public static Vector4 ReadVector4<T>(this ref T deserializer) where T : struct, IDeserializer {
    return new Vector4(deserializer.ReadSingle(), deserializer.ReadSingle(), 
                       deserializer.ReadSingle(), deserializer.ReadSingle());
  }

  /// <summary>
  /// Reads a Color from the underlying buffer. This will always be 16 bytes on
  /// the wire.
  /// </summary>
  public static Color ReadColor<T>(this ref T deserializer) where T : struct, IDeserializer {
    return new Color(deserializer.ReadSingle(), deserializer.ReadSingle(), 
                     deserializer.ReadSingle(), deserializer.ReadSingle());
  }

  /// <summary>
  /// Reads a Color32 from the underlying buffer. This will always be 4 bytes on
  /// the wire.
  /// </summary>
  public static Color32 ReadColor32<T>(this ref T deserializer) where T : struct, IDeserializer {
    return new Color32(deserializer.ReadByte(), deserializer.ReadByte(), 
                       deserializer.ReadByte(), deserializer.ReadByte());
  }

  /// <summary>
  /// Reads a Quaternion from the underlying buffer. This will always be 16 bytes
  /// on the wire.
  /// </summary>
  public static Quaternion ReadQuaternion<T>(this ref T deserializer) where T : struct, IDeserializer {
    return new Quaternion(deserializer.ReadSingle(), deserializer.ReadSingle(), 
                          deserializer.ReadSingle(), deserializer.ReadSingle());
  }

  /// <summary>
  /// Reads a Rect from the underlying buffer. This will always be 16 bytes
  /// on the wire.
  /// </summary>
  public static Rect ReadRect<T>(this ref T deserializer) where T : struct, IDeserializer {
    return new Rect(deserializer.ReadSingle(), deserializer.ReadSingle(), 
                    deserializer.ReadSingle(), deserializer.ReadSingle());
  }

  /// <summary>
  /// Reads a Plane from the underlying buffer. This will always be 16 bytes
  /// on the wire.
  /// </summary>
  public static Plane ReadPlane<T>(this ref T deserializer) where T : struct, IDeserializer {
    return new Plane(deserializer.ReadVector3(), deserializer.ReadSingle());
  }

  /// <summary>
  /// Reads a Ray from the underlying buffer. This will always be 24 bytes
  /// on the wire.
  /// </summary>
  public static Ray ReadRay<T>(this ref T deserializer) where T : struct, IDeserializer {
    return new Ray(deserializer.ReadVector3(), deserializer.ReadVector3());
  }

  /// <summary>
  /// Reads a Matrix4x4 from the underlying buffer. This will always be 64 bytes
  /// on the wire.
  /// </summary>
  public static Matrix4x4 ReadMatrix4x4<T>(this ref T deserializer) where T : struct, IDeserializer {
      Matrix4x4 m = new Matrix4x4();
      m.m00 = deserializer.ReadSingle();
      m.m01 = deserializer.ReadSingle();
      m.m02 = deserializer.ReadSingle();
      m.m03 = deserializer.ReadSingle();
      m.m10 = deserializer.ReadSingle();
      m.m11 = deserializer.ReadSingle();
      m.m12 = deserializer.ReadSingle();
      m.m13 = deserializer.ReadSingle();
      m.m20 = deserializer.ReadSingle();
      m.m21 = deserializer.ReadSingle();
      m.m22 = deserializer.ReadSingle();
      m.m23 = deserializer.ReadSingle();
      m.m30 = deserializer.ReadSingle();
      m.m31 = deserializer.ReadSingle();
      m.m32 = deserializer.ReadSingle();
      m.m33 = deserializer.ReadSingle();
      return m;
  }

  /// <summary>
  /// Reads a struct by directly copying it off of the buffer.
  /// </summary>
  public static void ReadStruct<T, TStruct>(this ref T deserializer, ref TStruct val) 
                                             where T : struct, IDeserializer
                                             where TStruct : struct {
    var size = UnsafeUtility.SizeOf<TStruct>();
    deserializer.Reserve(size);
    UnsafeUtility.CopyPtrToStructure(deserializer.Current, out val);
    deserializer.Current += size;
  }

  /// <summary>
  /// Reads a serializable message from the underlying buffer. If the type is a
  /// reference type, this will allocate GC.
  /// </summary>
  public static TMsg Read<T, TMsg>(this ref T deserializer) 
                                   where T : struct, IDeserializer
                                   where TMsg : struct, ISerializable {
    var msg = new TMsg();
    msg.Deserialize(ref deserializer);
    return msg;
  }

}

}