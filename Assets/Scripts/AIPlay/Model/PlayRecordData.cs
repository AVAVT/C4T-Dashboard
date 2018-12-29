using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayRecordData
{
  public string ISOTime;
  public SaveMapInfo mapInfo;
  public GameConfig gameRule;
  public Dictionary<Team, Dictionary<Role, string>> playerNames;
  public List<List<TurnAction>> turnActions;
}
