namespace HouraiTeahouse.Serialization {

/// <summary>
/// A serializable object.
/// </summary>
/// <remarks>
/// Typically this is implemented on a struct to minimize impact on GC.
/// </remarks>
public interface ISerializable {

  /// <summary>
  /// Serializes the object into a buffer via a serializer.
  ///
  /// The ordering of seriialziation must match the the ordering in Deserialize
  /// or there may bie issues in serialization parity.
  /// </summary>
  void Serialize<T>(ref T serializer) where T : ISerializer;

  /// <summary>
  /// Deserializes the object from a buffer via a deserializer.
  ///
  /// The ordering of seriialziation must match the the ordering in Serialize
  /// or there may be issues in serialization parity.
  /// </summary>
  void Deserialize<T>(ref T deserializer) where T : IDeserializer;

}

}
