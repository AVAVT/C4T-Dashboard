using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WebServiceTurnData
{
  public GameConfig gameRule;
  public GameState gameState;
  public int team;
  public int role;
}
