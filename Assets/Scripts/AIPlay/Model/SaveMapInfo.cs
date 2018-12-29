using System.Collections.Generic;
using System.Numerics;

[System.Serializable]
public class SaveMapInfo
{
  public Dictionary<Team, Dictionary<Role, Vector2>> startingPositions = new Dictionary<Team, Dictionary<Role, Vector2>>();
  public List<List<TileType>> tiles = new List<List<TileType>>();
}