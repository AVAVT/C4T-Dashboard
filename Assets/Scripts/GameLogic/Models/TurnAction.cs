[System.Serializable]
public struct TurnAction
{
  public Team team;
  public Role role;
  public string direction;
  public bool timedOut;
  public bool crashed;
  public TurnAction(Team team, Role role, string direction, bool timedOut = false, bool crashed = false)
  {
    this.team = team;
    this.role = role;
    this.direction = direction;
    this.timedOut = timedOut;
    this.crashed = crashed;
  }
}