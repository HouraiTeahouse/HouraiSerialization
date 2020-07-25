using System;

namespace HouraiTeahouse.Serialization {

/// <summary>
/// High speed, no/low GC deserializer reading from fixed size buffers that is
/// guarenteed to be conssistent regardless of platform.
/// </summary>
/// <remarks>
/// Calls to read data from the buffer do have bounds checking for safety
/// reasons.
///
/// Do not create these via "new FixedSizedDeserializer" or the program may crash from
/// segfaulting. Use FixedSizedDeserializer.Create instead.
///
/// This is a value type to avoid allocating GC, when passing it to other
/// functions, be sure to pass it by reference via ref parameters.
///
/// This struct does not lock access to the underlying buffer or the pointers to
/// it. Shared use across multiple threads is not safe. Copies of the same
/// deserializer is threadsafe, so long as there is no process writing to the
/// underlying buffer.
///
/// This deserializer favors small message size and compatibility over speed. 
/// If speed is imperative, it may be faster to directly copy
/// structs into the buffers. Such an alternative will likely not be portable as
/// it preserves the endianness of each value. Use in remote messaging may be
/// incorrect if the two cmmmunicating machines are using different endianness.
/// </remarks>
public unsafe struct FixedSizeDeserializer : IDeserializer {

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

  /// <summary>
  /// Creates a Deserializer from a provided buffer.
  /// </summary>
  public static FixedSizeDeserializer Create(byte* buf, uint size) {
    return new FixedSizeDeserializer {
      _start = buf,
      _current = buf,
      _end = buf + size,
    };
  }

  /// <summary>
  /// Creates a Deserializer from a provided FixedBuffer.
  /// </summary>
  public static FixedSizeDeserializer Create(ReadOnlySpan<byte> buf) {
    fixed (byte* ptr = buf) {
      return Create(ptr, (uint)buf.Length);
    }
  }

  /// <summary>
  /// Deserializes an object directly from a base64 string.
  ///
  /// This function allocates GC.
  public static T FromBase64String<T>(string encoded) where T : ISerializable, new() {
    var bytes = Convert.FromBase64String(encoded);
    fixed (byte* ptr = bytes) {
      var obj = new T();
      var deserializer = Create(ptr, (uint)bytes.Length);
      obj.Deserialize(ref deserializer);
      return obj;
    }
  }

}

}
