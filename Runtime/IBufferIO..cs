using System;

namespace HouraiTeahouse.Serialization {

public unsafe interface IBufferIO : IDisposable {
  byte* Start { get; }
  byte* End { get; }
  byte* Current { get; set; }

  void Reserve(int count);
}

public static unsafe class IBufferIOExtensions {

  public static bool IsValid<T>(this ref T io) where T : struct, IBufferIO {
    return io.Start != null && io.End != null && io.Current != null &&
      io.Current >= io.Start && io.Current < io.End;
  }

  public static int GetPosition<T>(this ref T io) where T : struct, IBufferIO
    => (int)(io.End - io.Current);
  public static int GetSize<T>(this ref T io) where T : struct, IBufferIO
    => (int)(io.End - io.Start);

  public static void SeekZero<T>(this ref T io) where T : struct, IBufferIO {
    io.Current = io.Start;
  }

}

}