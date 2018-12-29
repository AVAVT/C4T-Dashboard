using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class TeamRoleMapConverter<T> : JsonConverter
{
  public override bool CanConvert(Type objectType)
  {
    if (objectType == typeof(TeamRoleMap<>)) return true;
    else return false;
  }

  public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
  {
    JObject jObject = JObject.Load(reader);
    var map = jObject.ToObject<Dictionary<Team, Dictionary<Role, T>>>();
    return new TeamRoleMap<T>(map);
  }

  public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
  {
    writer.WriteRawValue((value as TeamRoleMap<T>).ToJSON());
    writer.WriteEndObject();
  }
}