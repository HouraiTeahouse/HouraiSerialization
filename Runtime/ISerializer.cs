using System;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections.LowLevel.Unsafe;

namespace HouraiTeahouse.Serialization {

public unsafe interface ISerializer : IBufferIO {
}

public unsafe static class ISerializerExtensions {

#if !INCLUDE_IL2CPP
  static UIntFloat s_FloatConverter;
#endif
  
  public static ReadOnlySpan<byte> AsReadOnlySpan<T>(this ref T serializer) where T : struct, ISerializer {
    Assert.IsTrue(serializer.IsValid());
    return new ReadOnlySpan<byte>(serializer.Start, serializer.GetSize());
  }

  public static string ToBase64String<T>(this ref T serializer) where T : struct, ISerializer {
    Assert.IsTrue(serializer.IsValid());
    return Convert.ToBase64String(serializer.AsReadOnlySpan().ToArray());
  }

  public static void Write<T>(this ref T serializer, byte value) where T : struct, ISerializer {
    Assert.IsTrue(serializer.IsValid());
    *serializer.Current++ = value;
  }

  public static void Write<T>(this ref T serializer, ushort value) where T : struct, ISerializer {
    Assert.IsTrue(serializer.IsValid());
    byte* current = null;
    // http://sqlite.org/src4/doc/trunk/www/varint.wiki
    if (value <= 240) {
      serializer.Reserve(1);
      current = serializer.Current;
      *current++ = (byte)value;
    } else if (value <= 2287) {
      serializer.Reserve(2);
      current = serializer.Current;
      *current++ = (byte)((value - 240) / 256 + 241);
      *current++ = (byte)((value - 240) % 256);
    } else {
      serializer.Reserve(3);
      current = serializer.Current;
      *current++ = (byte)249;
      *current++ = (byte)((value - 2288) / 256);
      *current++ = (byte)((value - 2288) % 256);
    }
    serializer.Current = current;
  }

  public static void Write<T>(this ref T serializer, uint value) where T : struct, ISerializer {
    Assert.IsTrue(serializer.IsValid());
    byte* current = null;
    // http://sqlite.org/src4/doc/trunk/www/varint.wiki
    if (value <= 240) {
      serializer.Reserve(1);
      current = serializer.Current;
      *current++ = (byte)value;
    } else if (value <= 2287) {
      serializer.Reserve(2);
      current = serializer.Current;
      *current++ = (byte)((value - 240) / 256 + 241);
      *current++ = (byte)((value - 240) % 256);
    } else if (value <= 67823) {
      serializer.Reserve(3);
      current = serializer.Current;
      *current++ = (byte)249;
      *current++ = (byte)((value - 2288) / 256);
      *current++ = (byte)((value - 2288) % 256);
    } else if (value <= 16777215) {
      serializer.Reserve(4);
      current = serializer.Current;
      *current++ = (byte)250;
      *current++ = (byte)value;
      *current++ = (byte)(value >> 8);
      *current++ = (byte)(value >> 16);
    } else {
      serializer.Reserve(5);
      current = serializer.Current;
      *current++ = (byte)251;
      *current++ = (byte)value;
      *current++ = (byte)(value >> 8);
      *current++ = (byte)(value >> 16);
      *current++ = (byte)(value >> 24);
    }
    serializer.Current = current;
  }

  public static void Write<T>(this ref T serializer, ulong value) where T : struct, ISerializer {
    Assert.IsTrue(serializer.IsValid());
    byte* current = null;
    // http://sqlite.org/src4/doc/trunk/www/varint.wiki
    if (value <= 240) {
      serializer.Reserve(1);
      current = serializer.Current;
      serializer.Write((byte)value);
    } else if (value <= 2287) {
      serializer.Reserve(1);
      current = serializer.Current;
      *current++ = (byte)((value - 240) / 256 + 241);
      *current++ = (byte)((value - 240) % 256);
    } else if (value <= 67823) {
      serializer.Reserve(1);
      current = serializer.Current;
      *current++ = (byte)249;
      *current++ = (byte)((value - 2288) / 256);
      *current++ = (byte)((value - 2288) % 256);
    } else if (value <= 16777215) {
      serializer.Reserve(1);
      current = serializer.Current;
      *current++ = (byte)250;
      *current++ = (byte)value;
      *current++ = (byte)(value >> 8);
      *current++ = (byte)(value >> 16);
    } else if (value <= 4294967295) {
      serializer.Reserve(1);
      current = serializer.Current;
      *current++ = (byte)251;
      *current++ = (byte)value;
      *current++ = (byte)(value >> 8);
      *current++ = (byte)(value >> 16);
      *current++ = (byte)(value >> 24);
    } else if (value <= 1099511627775) {
      serializer.Reserve(1);
      current = serializer.Current;
      *current++ = (byte)252;
      *current++ = (byte)value;
      *current++ = (byte)(value >> 8);
      *current++ = (byte)(value >> 16);
      *current++ = (byte)(value >> 24);
      *current++ = (byte)(value >> 32);
    } else if (value <= 281474976710655) {
      serializer.Reserve(1);
      current = serializer.Current;
      *current++ = (byte)253;
      *current++ = (byte)value;
      *current++ = (byte)(value >> 8);
      *current++ = (byte)(value >> 16);
      *current++ = (byte)(value >> 24);
      *current++ = (byte)(value >> 32);
      *current++ = (byte)(value >> 40);
    } else if (value <= 72057594037927935) {
      serializer.Reserve(1);
      current = serializer.Current;
      *current++ = (byte)254;
      *current++ = (byte)value;
      *current++ = (byte)(value >> 8);
      *current++ = (byte)(value >> 16);
      *current++ = (byte)(value >> 24);
      *current++ = (byte)(value >> 32);
      *current++ = (byte)(value >> 40);
      *current++ = (byte)(value >> 48);
    } else {
      serializer.Reserve(1);
      current = serializer.Current;
      *current++ = (byte)255;
      *current++ = (byte)value;
      *current++ = (byte)(value >> 8);
      *current++ = (byte)(value >> 16);
      *current++ = (byte)(value >> 24);
      *current++ = (byte)(value >> 32);
      *current++ = (byte)(value >> 40);
      *current++ = (byte)(value >> 48);
      *current++ = (byte)(value >> 56);
    }
    serializer.Current = current;
  }

  public static void Write<T>(this ref T serializer, sbyte value) where T : struct, ISerializer {
    serializer.Write((byte)value);
  }

  public static void Write<T>(this ref T serializer, short value) where T : struct, ISerializer {
    serializer.Write((UInt16)ZigZag.Encode(value, 16));
  }

  public static void Write<T>(this ref T serializer, int value) where T : struct, ISerializer {
    serializer.Write((UInt16)ZigZag.Encode(value, 32));
  }

  public static void Write<T>(this ref T serializer, long value) where T : struct, ISerializer {
    serializer.Write(ZigZag.Encode(value, 64));
  }

  public static void Write<T>(this ref T serializer, float value) where T : struct, ISerializer {
#if INCLUDE_IL2CPP
    serializer.Write(BitConverter.ToUInt32(BitConverter.GetBytes(value), 0));
#else
    s_FloatConverter.floatValue = value;
    serializer.Write(s_FloatConverter.intValue);
#endif
  }

  public static void Write<T>(this ref T serializer, double value) where T : struct, ISerializer {
#if INCLUDE_IL2CPP
    serializer.Write(BitConverter.ToUInt64(BitConverter.GetBytes(value), 0));
#else
    s_FloatConverter.doubleValue = value;
    serializer.Write(s_FloatConverter.longValue);
#endif
  }

  public static void Write<T>(this ref T serializer, string value) where T : struct, ISerializer {
    if (value == null) {
      *serializer.Current++ = 0;
      *serializer.Current++ = 0;
      return;
    }

    var encoding = SerializationConstants.Encoding;
    int count = encoding.GetByteCount(value);
    serializer.Write((ushort)(count));
    serializer.Write(count + 1);
    fixed (char* charPtr = value.ToCharArray()) {
      serializer.Current += encoding.GetBytes(
            charPtr, value.Length, serializer.Current, 
            (int)(serializer.End - serializer.Current));
    }
  }

  public static void Write<T>(this ref T serializer, bool value) where T : struct, ISerializer {
    serializer.Write((byte)(value ? 1 : 0));
  }

  public static void Write<T>(this ref T serializer, byte[] buffer, ushort count) where T : struct, ISerializer {
    serializer.Reserve(count);
    fixed (byte* bufPtr = buffer) {
      UnsafeUtility.MemCpy(serializer.Current, bufPtr, count);
    }
    serializer.Current += count;
  }

  public static void Write<T>(this ref T serializer, byte* buffer, ushort count) where T : struct, ISerializer {
    serializer.Reserve(count);
    UnsafeUtility.MemCpy(serializer.Current, buffer, count);
    serializer.Current += count;
  }

  public static void WriteBytesAndSize<T>(this ref T serializer, byte[] buffer, ushort count) where T : struct, ISerializer {
    if (buffer == null || count == 0) {
      serializer.Write((ushort)0);
      return;
    }

    serializer.Write(count);
    serializer.Write(buffer, count);
  }

  public static void WriteStruct<T, TValue>(this ref T serializer, ref TValue value) 
                                            where T : struct, ISerializer 
                                            where TValue : struct {
    var size = UnsafeUtility.SizeOf<TValue>();
    serializer.Reserve(size);
    UnsafeUtility.CopyStructureToPtr(ref value, serializer.Current);
    serializer.Current += size;
  }

  public static void WriteStruct<T, TValue>(this ref T serializer, void* buffer, int count) 
                                             where T : struct, ISerializer 
                                             where TValue : struct {
    var size = UnsafeUtility.SizeOf<TValue>();
    serializer.Reserve(size);
    UnsafeUtility.MemCpy(serializer.Current, buffer, size * count);
    serializer.Current += count;
  }

  public static void Write<T>(this ref T serializer, Vector2 value) 
                              where T : struct, ISerializer {
    serializer.Write(value.x);
    serializer.Write(value.y);
  }

  public static void Write<T>(this ref T serializer, Vector3 value) 
                              where T : struct, ISerializer {
    serializer.Write(value.x);
    serializer.Write(value.y);
    serializer.Write(value.z);
  }

  public static void Write<T>(this ref T serializer, Vector4 value) 
                              where T : struct, ISerializer {
    serializer.Write(value.x);
    serializer.Write(value.y);
    serializer.Write(value.z);
    serializer.Write(value.w);
  }

  public static void Write<T>(this ref T serializer, Color value) 
                              where T : struct, ISerializer {
    serializer.Write(value.r);
    serializer.Write(value.g);
    serializer.Write(value.b);
    serializer.Write(value.a);
  }

  public static void Write<T>(this ref T serializer, Color32 value) 
                              where T : struct, ISerializer {
    serializer.Reserve(4);
    byte* current = serializer.Current;
    *current++ = value.r;
    *current++ = value.g;
    *current++ = value.b;
    *current++ = value.a;
    serializer.Current = current;
  }

  public static void Write<T>(this ref T serializer, Quaternion value) 
                              where T : struct, ISerializer {
    serializer.Write(value.x);
    serializer.Write(value.y);
    serializer.Write(value.z);
    serializer.Write(value.w);
  }

  public static void Write<T>(this ref T serializer, Rect value) 
                              where T : struct, ISerializer {
    serializer.Write(value.xMin);
    serializer.Write(value.yMin);
    serializer.Write(value.width);
    serializer.Write(value.height);
  }

  public static void Write<T>(this ref T serializer, Plane value) 
                              where T : struct, ISerializer {
    serializer.Write(value.normal);
    serializer.Write(value.distance);
  }

  public static void Write<T>(this ref T serializer, Ray value) 
                              where T : struct, ISerializer {
    serializer.Write(value.direction);
    serializer.Write(value.origin);
  }

  public static void Write<T>(this ref T serializer, Matrix4x4 value) 
                              where T : struct, ISerializer {
    serializer.Write(value.m00);
    serializer.Write(value.m01);
    serializer.Write(value.m02);
    serializer.Write(value.m03);
    serializer.Write(value.m10);
    serializer.Write(value.m11);
    serializer.Write(value.m12);
    serializer.Write(value.m13);
    serializer.Write(value.m20);
    serializer.Write(value.m21);
    serializer.Write(value.m22);
    serializer.Write(value.m23);
    serializer.Write(value.m30);
    serializer.Write(value.m31);
    serializer.Write(value.m32);
    serializer.Write(value.m33);
  }

  public static void Write<T, TValue>(this ref T serializer, TValue value) 
                                      where T : struct, ISerializer 
                                      where TValue : ISerializable {
    value.Serialize(ref serializer);
  }

}

}
