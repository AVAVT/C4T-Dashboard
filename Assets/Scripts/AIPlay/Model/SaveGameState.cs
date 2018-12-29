using System.Collections.Generic;

[System.Serializable]
public class SaveGameState
{
  public int turn = 0;
  public int redScore = 0;
  public int blueScore = 0;
  public Dictionary<Team, Dictionary<Role, Character>> characters = new Dictionary<Team, Dictionary<Role, Character>>();
  public List<List<Tile>> map = new List<List<Tile>>();
}