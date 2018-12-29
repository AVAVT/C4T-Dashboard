using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayRecordData
{
  public string ISOTime;
  public MapInfo mapInfo;
  public GameConfig gameRule;
  public TeamRoleMap<string> playerNames;
  public List<List<TurnAction>> turnActions;
}
