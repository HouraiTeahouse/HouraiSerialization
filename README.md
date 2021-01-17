# Hourai Serialization

High performance binary message serialization for Unity 2018.3+.

## Features

 * High performance message serialization. Intended and optimized for game netcode communications.
 * Efficent encoding for optimizing message sizes. Useful for minimizing bandwidth and memory usage.
 * (Next to) Zero garbage collection.
 * Written in pure C#: Portable to all platforms Unity supports.

## Installation
Hourai Serialization is most easily installable via Unity Package Manager. In Unity 2018.3+,
add the following to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.houraiteahouse.serialization": "0.1.1"
  },
  "scopedRegistries": [
    {
      "name": "Hourai Teahouse",
      "url": "https://upm.houraiteahouse.net",
      "scopes": ["com.houraiteahouse"]
    }
  ]
}
```

## Usage

NOTE: All of the types in this library are structs and should NEVER be constructed
with the `new` operator. Use the approriate `Create` static methods for each type
instead.

## Encoding

All serializers/deserializers sequentially encode or decode values in the underlying
buffer, and support efficiently serializing many different types:

 * Numeric types - `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`
   `float`
 * Strings - Encoded using UTF-8.
 * Byte blobs - `byte[]` or `byte*`
 * Unity Types - `Vector2`, `Vector3`, `Vector4`, `Quaternion`, `Matrix4x4`, etc.

All integer types are encoded as variable length integers using the format used by
[SQLite](https://sqlite.org/src4/doc/trunk/www/varint.wiki) to save space when
encoding small values. Signed integers (`short`, `int`, `long`) are transformed into
unsigned values via
[ZigZag encoding](https://developers.google.com/protocol-buffers/docs/encoding#signed_integers).

### Serialization
The simplest and highest performance way of serializing messages is to use
`FixedSizeSerializer` with stack allocated byte buffers. Use this if the maximum message
size is known ahead of time, and the message size is assured to be small enough to
fit on the stack (usually 1MB for Unity/C# applications).

```csharp
// Allocate a buffer on the stack of size 1024 and wrap it in a serializer:
Span<byte> buffer = stackalloc byte[1024];
var serializer = FixedSizeSerializer.Create(buffer);
```

If the size of the encoded message is unknown, unbounded, or expected to be larger
than the stack, use `DynamicSerializer` instead. This serializer allocates a
buffer on the heap and grows the buffer each time the message grows too large.
The buffer has manually managed memory, so the serializer must be manually disposed
of:

```csharp
// Allocate a growable buffer with an initial size of 1024 and wrap it in a serializer:
using (var serializer = DynamicSerializer.Create(1024)) {
  ...
}
```

To write values with the serializer, use the overloaded `Write` method:

```csharp
serializer.Write(100);            // Integers
serializer.Write("Hello world!"); // Strings
serializer.Write(Vector3.up);     // Unity types
```

## Deserialization
Deserialization tends to be easier: the length of the buffer is already known.
Convert the encoded message into a `Span<byte>` or a read only span, and use it to
create a `FixedSizeDeserializer`:

```csharp
byte[] message = GetMessage(...);
var deserializer = FixedSizeDeserializer.Create(new ReadOnlySpan<byte>(message));
```

The deserializer can then be used to sequentially read values from the buffer:

```csharp
int intVal = deserializer.ReadInt32();            // Integers
string stringVal = deserializer.ReadString();     // Strings
Vector3 vectorVal = deserializer.ReadVector3();   // Unity types
```

## Serializing/Deserializing Custom Types
Games require more than just simple types. It's often required to serialize complex
message types. For this, the `ISerializable` interface allows for easy identification
and integration of serialization of custom types. A complete example is show below:

```csharp
public struct GameInput : ISerializable {
  public Vector2 Movement;
  public bool AttackButton;
  public bool SpecialButton;
  public bool JumpButton;
  public bool ShieldButton;

  // Require for ISerializable
  public void Serialize<T>(ref T serializer) where T : struct, ISerializer {
    serializer.Write(Movement);
    serializer.Write(AttackButton);
    serializer.Write(SpecialButton);
    serializer.Write(JumpButton);
    serializer.Write(SheildButton);
  }

  // Require for ISerializable
  public void Deserialize<T>(ref T deserializer) where T : struct, IDeserializer {
    // Note how this order mirrors what is done in Serialize
    Movement = deserializer.ReadVector2();
    AttackButton = deserializer.ReadBool();
    SpecialButton = deserializer.ReadBool();
    JumpButton = deserializer.ReadBool();
    SheildButton = deserializer.ReadBool();
  }

}
```

This allows for deeper integration as there are generic functions for reading and
writing custom types `serializer.Write<T>` and `deserializer.Read<T>` for types that
derive from `ISerializalble`.

## FAQ

**Why use this over Cap'n Proto, Flatbuffers, Protobuffers, ZeroFormatter, etc?**

All of those formats are targetted towards general use and high level safety, often
trading off performance for increased safety annd flexibility. For video games,
performance is often paramount. The average link MTU is 1500 bytes for most network
connections. Any larger and a network packet may need to be manually fragmented into
smaller parts.
