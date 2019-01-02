[System.Serializable]
public class TurnAction
{
  public Team team;
  public Role role;
  public string direction;
  public string actualDirection;
  public bool timedOut;
  public bool crashed;
  public TurnAction(Team team, Role role, string direction, bool timedOut = false, bool crashed = false)
  {
    this.team = team;
    this.role = role;
    this.direction = direction;
    this.actualDirection = direction;
    this.timedOut = timedOut;
    this.crashed = crashed;
  }
}