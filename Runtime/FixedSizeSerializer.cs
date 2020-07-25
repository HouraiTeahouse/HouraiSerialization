using System;

namespace HouraiTeahouse.Serialization {

/// <summary>
/// High speed, no/low GC serializer for writing to fixed size buffers that is
/// guarenteed to be conssistent regardless of platform.
/// </summary>
/// <remarks>
/// The underlying buffer is allocated before the serializer, is not owned by 
/// the serializer, and thus must outlive the serializer itself. 
/// 
/// Writing more data than the underlying buffer contains will throw errors.
/// The buffer will not be resized.
/// 
/// Calls to write data from the buffer do have bounds checking for safety
/// reasons.
///
/// Do not create these via "new FixedSizeSerializer" or the program may crash 
/// from segfaults. Use FixedSizeSerializer.Create instead.
///
/// This is a value type to avoid allocating GC, when passing it to other
/// functions, be sure to pass it by reference via ref parameters.
///
/// This struct does not lock access to the underlying buffer or the pointers to
/// it. Shared use across multiple threads is not safe. 
/// 
/// This type is optimally used with stackalloc'ed byte buffers for optimal
/// performance. If the size of the serialized buffer is not known ahead
/// of time, DynamicSerializer may be a better choice, as it will dynamically
/// grow the size of the buffer to fit the data being serialized.
///
/// This serializer favors small message size and compatibility over speed. 
/// If speed is imperative, it may be faster to directly copy structs into 
/// buffers. Such an alternative will likely not be portable as it preserves 
/// the endianness of each value. Use in remote messaging may be incorrect if 
/// the two cmmmunicating machines are using different endianness.
/// </remarks>
public unsafe struct FixedSizeSerializer : ISerializer {

  byte* _start, _current, _end;

  byte* IBufferIO.Start => _start;
  byte* IBufferIO.End => _end;
  byte* IBufferIO.Current {
    get { return _current; }
    set { _current = value; }
  }

  void IBufferIO.Reserve(int size) {
    if (_current + size > _end) {
      throw new IndexOutOfRangeException("Buffer overflow: " + ToString());
    }
  }

  void IDisposable.Dispose() {}

  public static FixedSizeSerializer Create(Span<byte> buffer) {
    fixed (byte* ptr = buffer) {
      return new FixedSizeSerializer {
          _start = ptr,
          _current = ptr,
          _end = ptr + buffer.Length,
      };
    }
  }

}

}
