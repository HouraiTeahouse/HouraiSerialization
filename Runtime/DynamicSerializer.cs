using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace HouraiTeahouse.Serialization {

/// <summary>
/// High speed, no/low GC serializer for writing to fixed size buffers that is
/// guarenteed to be conssistent regardless of platform.
/// </summary>
/// <remarks>
/// The underlying buffer is allocated by the serializer, is owned by 
/// the serializer, and must be disposed of, via Dispose(), when the 
/// serializer's lifetime ends. The buffer is allocated with UnsafeUtility
/// methods which must be manually managed, so failing to dispose of the 
/// serializer will cause a memory leak.
/// 
/// Calls to write data from the buffer do have bounds checking for safety
/// reasons. Writing more data than the underlying buffer contains will 
/// reallocate a larger buffer at double the size and copy the existing 
/// contents bup to an optional maximum size.
///
/// Do not create these via "new DynamicSerializer" or the program may crash 
/// from segfaults. Use DynamicSerializer.Create instead.
///
/// This is a value type to avoid allocating GC, when passing it to other
/// functions, be sure to pass it by reference via ref or in parameters.
///
/// This struct does not lock access to the underlying buffer or the pointers to
/// it. Shared use across multiple threads is not safe. 
/// 
/// If the maximum size of the buffer is known and can be preallcoated, it's
/// advised to use FixedSizeSerializer with stackalloc instead.
///
/// This serializer favors small message size and compatibility over speed. 
/// If speed is imperative, it may be faster to directly copy structs into 
/// buffers. Such an alternative will likely not be portable as it preserves 
/// the endianness of each value. Use in remote messaging may be incorrect if 
/// the two cmmmunicating machines are using different endianness.
/// </remarks>
public unsafe struct DynamicSerializer : ISerializer {

  int _maxSize;
  Allocator _allocator;
  byte* _start, _current, _end;

  byte* IBufferIO.Start => _start;
  byte* IBufferIO.End => _end;
  byte* IBufferIO.Current {
    get { return _current; }
    set { _current = value; }
  }

  void IBufferIO.Reserve(int size) {
    if (_current + size <= _end) return;
    var position = _current - _start;
    int currentSize = this.GetSize();
    int newSize = currentSize;

    while (newSize < currentSize + size) {
      newSize *= 2;
      if (_maxSize >= 0 && newSize > _maxSize) {
        throw new InvalidOperationException($"Cannot grow underlying buffer to be larger than {_maxSize} bytes");
      }
    }

    var newBuffer = AllocateBuffer(newSize, _allocator);
    UnsafeUtility.MemCpy(newBuffer, _start, currentSize);
    UnsafeUtility.Free(_start, _allocator);

    _start = newBuffer;
    _current = newBuffer + position;
    _end = newBuffer + newSize;
  }

  /// <summary>
  /// Creates and allocates a DynamicSerializer on the heap.
  /// </summary>
  /// <remarks>
  /// Allocates an initial buffer of size initialSize.
  /// 
  /// For performance reasons, the underlying buffer is not cleared when it's
  /// allocated.
  /// </remarks>
  /// <param name="initialSize">the initial starting size of the buffer.</param>
  /// <param name="maxSize">the maximum size the buffer can grow to.</param>
  /// <param name="allocator">the Unity allocator to use when allocating the underlying buffer.</param>
  /// <returns></returns>
  public static DynamicSerializer Create(int initialSize, int maxSize = -1, 
                                           Allocator allocator = Allocator.Temp) {
    byte* ptr = AllocateBuffer(initialSize, allocator);
    return new DynamicSerializer {
        _start = ptr,
        _current = ptr,
        _end = ptr + initialSize,
        _maxSize = maxSize,
        _allocator = allocator
    };
  }

  static byte* AllocateBuffer(int size, Allocator allocator) {
    return (byte*)UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<byte>(), allocator);
  }

  public void Dispose() {
    UnsafeUtility.Free(_start, _allocator);
  }

}

}
