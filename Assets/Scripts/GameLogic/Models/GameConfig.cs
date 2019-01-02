using System.Collections.Generic;
using System.Numerics;

[System.Serializable]
public class GameConfig
{
  public int sightDistance;
  public int gameLength;
  public int plantFruitTime;
  public int wildberryFruitTime;
  public int harvesterMaxCapacity;
  public Dictionary<TileType, int> fruitScoreValues = new Dictionary<TileType, int>();
  public List<Team> availableTeams;
  public List<Role> availableRoles;

  public static GameConfig DefaultGameRule()
  {
    return new GameConfig(
      sightDistance: 2,
      gameLength: 500,
      plantFruitTime: 5,
      wildberryFruitTime: 10,
      fruitHarvestValue: 1,
      wildberryHarvestValue: 2,
      harvesterMaxCapacity: 5,
      availableTeams: new List<Team>() { Team.Red, Team.Blue },
      availableRoles: new List<Role>() { Role.Planter, Role.Harvester, Role.Worm }
    );
  }

  public GameConfig()
  {
  }

  public GameConfig(int sightDistance, int gameLength, int plantFruitTime, int wildberryFruitTime, int fruitHarvestValue, int wildberryHarvestValue, int harvesterMaxCapacity, List<Team> availableTeams, List<Role> availableRoles)
  {
    this.sightDistance = sightDistance;
    this.gameLength = gameLength;
    this.plantFruitTime = plantFruitTime;
    this.wildberryFruitTime = wildberryFruitTime;
    this.harvesterMaxCapacity = harvesterMaxCapacity;
    this.fruitScoreValues = new Dictionary<TileType, int>(){
      {TileType.TOMATO, fruitHarvestValue},
      {TileType.PUMPKIN, fruitHarvestValue},
      {TileType.WILDBERRY, wildberryHarvestValue},
    };
    this.availableTeams = availableTeams;
    this.availableRoles = availableRoles;
  }

  public TileType ScoreTileTypeForTeam(Team team) => team == Team.Red ? TileType.RED_BOX : TileType.BLUE_BOX;
  public TileType FruitTileTypeForTeam(Team team) => team == Team.Red ? TileType.TOMATO : TileType.PUMPKIN;
  public List<TileType> WormDestroyTileTypeForTeam(Team team) => new List<TileType>() { team == Team.Red ? TileType.PUMPKIN : TileType.TOMATO };
}