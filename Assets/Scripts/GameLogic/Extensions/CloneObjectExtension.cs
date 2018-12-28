using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public static class ObjectExtensions
{
  public static T Clone<T>(this T source)
  {
    if (!typeof(T).IsSerializable)
    {
      throw new ArgumentException("The type must be serializable.", "source");
    }

    // Don't serialize a null object, simply return the default for that object
    if (Object.ReferenceEquals(source, null))
    {
      return default(T);
    }

    using (var ms = new MemoryStream())
    {
      var formatter = new BinaryFormatter();
      formatter.Serialize(ms, source);
      ms.Position = 0;

      return (T)formatter.Deserialize(ms);
    }
  }
}