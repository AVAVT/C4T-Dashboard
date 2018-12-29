using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

[System.Serializable]
public class TeamRoleMap<T> : IEnumerable<T>, ISerializable
{
  private Dictionary<Team, Dictionary<Role, T>> map;
  private IEnumerable<T> flattenedMap;

  public TeamRoleMap()
  {
    map = new Dictionary<Team, Dictionary<Role, T>>();
    flattenedMap = new List<T>();
  }

  public TeamRoleMap(Dictionary<Team, Dictionary<Role, T>> map)
  {
    this.map = map;
    UpdateFlattenedList();
  }

  public Dictionary<Team, Dictionary<Role, T>> ToDictionary()
  {
    return new Dictionary<Team, Dictionary<Role, T>>(map);
  }

  public T GetItem(Team team, Role role)
  {
    return map[team][role];
  }

  public Dictionary<Role, T> GetItemsBy(Team team)
  {
    Dictionary<Role, T> result = new Dictionary<Role, T>();
    if (map.ContainsKey(team))
    {
      foreach (var kvp in map[team])
      {
        result[kvp.Key] = kvp.Value;
      }
    }

    return result;
  }

  public IEnumerable<T> GetItemsBy(Role role)
  {
    List<T> result = new List<T>();
    foreach (var kvp in map)
    {
      if (kvp.Value.ContainsKey(role)) result.Add(kvp.Value[role]);
    }

    return result;
  }

  public IEnumerable<Team> GetTeams()
  {
    return map.Keys;
  }

  public void SetItem(Team team, Role role, T item)
  {
    if (!map.ContainsKey(team)) map[team] = new Dictionary<Role, T>();
    map[team][role] = item;

    UpdateFlattenedList();
  }

  public bool DeleteItem(Team team, Role role)
  {
    if (map.ContainsKey(team) && map[team].ContainsKey(role))
    {
      var result = map[team].Remove(role);
      UpdateFlattenedList();
      return result;
    }
    else return false;
  }

  public void ReplaceWithItemFrom(TeamRoleMap<T> other, Team team, Role role)
  {
    SetItem(team, role, other.GetItem(team, role));
  }

  public string ToJSON()
  {
    return JsonConvert.SerializeObject(map);
  }

  void UpdateFlattenedList()
  {
    flattenedMap = map.SelectMany(d => d.Value).Select(kvp => kvp.Value);
  }

  public IEnumerator<T> GetEnumerator()
  {
    return flattenedMap.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return flattenedMap.GetEnumerator();
  }

  public void GetObjectData(SerializationInfo info, StreamingContext context)
  {
    info.AddValue("map", map, typeof(Dictionary<Team, Dictionary<Role, T>>));
  }

  public TeamRoleMap(SerializationInfo info, StreamingContext context)
  {
    map = (Dictionary<Team, Dictionary<Role, T>>)info.GetValue("map", typeof(Dictionary<Team, Dictionary<Role, T>>));
    UpdateFlattenedList();
  }
}