using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GameLogic
{
  private ServerGameState serverGameState;

  private static int version = 1;

  MapInfo mapInfo;
  GameConfig gameRule;
  public TeamRoleMap<ICharacterDescisionMaker> characterControllers;

  public static GameLogic GameLogicForNewGame(GameConfig gameRule, MapInfo mapInfo)
  {
    return new GameLogic(gameRule, mapInfo);
  }

  public static GameLogic GameLogicFromArbitaryState(GameConfig gameRule, ServerGameState currentGameState)
  {
    return new GameLogic(gameRule, currentGameState);
  }

  public ServerGameState GetGameStateSnapShot()
  {
    return serverGameState.Clone();
  }

  private GameLogic(GameConfig gameRule, MapInfo mapInfo)
  {
    serverGameState = new ServerGameState();
    this.mapInfo = mapInfo;
    this.gameRule = gameRule;
    InitializeCharacters(serverGameState, mapInfo);
    InitializeMap(serverGameState, mapInfo);
  }

  private GameLogic(GameConfig gameRule, ServerGameState currentGameState)
  {
    serverGameState = currentGameState;
    this.gameRule = gameRule;
  }

  public void BindDecisionMakers(TeamRoleMap<ICharacterDescisionMaker> decisionMakers)
  {
    this.characterControllers = decisionMakers;

    AssignCharacterToControllers(serverGameState, decisionMakers);
  }

  /// <summary>Play a full game from beginning to end with provided recorder</summary>
  public async Task PlayGame(CancellationToken cancellationToken, IReplayRecorder recorder = null)
  {
    try
    {
      await DoStart(recorder);
      while (serverGameState.turn < gameRule.gameLength && !cancellationToken.IsCancellationRequested)
      {
        await PlayNextTurn(recorder);
      }

      DoEnd(recorder);

      return;
    }
    catch (System.Exception e)
    {
      throw e;
    }
  }

  async Task DoStart(IReplayRecorder recorder)
  {
    try
    {
      foreach (var team in characterControllers.GetTeams())
      {
        GameState teamGameState = serverGameState.GameStateForTeam(team, gameRule);

        foreach (var controller in characterControllers.GetItemsBy(team).Values)
        {
          await controller.DoStart(teamGameState, gameRule);
        }
      }
      recorder?.LogGameStart(version, gameRule, mapInfo);
    }
    catch (System.Exception e) { throw e; }
  }

  async Task PlayNextTurn(IReplayRecorder recorder)
  {
    try
    {
      List<TurnAction> actions = new List<TurnAction>();

      foreach (var team in characterControllers.GetTeams())
      {
        var teamGameState = serverGameState.GameStateForTeam(team, gameRule);

        foreach (var controller in characterControllers.GetItemsBy(team).Values)
        {
          var result = await controller.DoTurn(teamGameState, gameRule);
          actions.Add(new TurnAction(
            controller.Character.team,
            controller.Character.role,
            result,
            controller.IsTimedOut,
            controller.IsCrashed
          ));
        }
      }

      ExecuteTurn(actions);
      recorder?.LogTurn(serverGameState, actions);
    }
    catch (System.Exception e) { throw e; }
  }

  /// <summary>Progress the game state with given actions</summary>
  public void ExecuteTurn(List<TurnAction> actions)
  {
    ResetWormCaughtState();
    DoMove(actions);
    DoCatchWorm(serverGameState);
    DoScareHarvester(serverGameState);
    DoPlantTree(serverGameState);
    DoGetPoint(serverGameState);
    DoDestroyPlant(serverGameState);
    DoGrowPlant(serverGameState);
    DoHarvest(serverGameState);
    serverGameState.turn++;
  }

  void DoEnd(IReplayRecorder recorder)
  {
    recorder?.LogEndGame(serverGameState);
  }

  void ResetWormCaughtState()
  {
    foreach (var worm in serverGameState.characters.GetItemsBy(Role.Worm))
    {
      worm.isCaught = false;
    }
  }

  void DoMove(List<TurnAction> actions)
  {
    TeamRoleMap<Character> targetPoses = new TeamRoleMap<Character>();
    targetPoses = serverGameState.characters.Clone();
    foreach (var action in actions)
    {
      var character = targetPoses.GetItem(action.team, action.role);
      var newPos = new Vector2(character.x, character.y);
      if (!character.isScared)
        newPos += action.direction.ToDirectionVector();
      else
        character.isScared = false;

      character.x = Math.Max(Math.Min((int)newPos.X, mapInfo.tiles.Count - 1), 0);
      character.y = Math.Max(Math.Min((int)newPos.Y, mapInfo.tiles[character.x].Count - 1), 0);

      if (IsInImpassableTile(serverGameState, character))
      {
        targetPoses.ReplaceWithItemFrom(serverGameState.characters, character.team, character.role);
      }
    }

    CancelMovementForCounterRolesSwapingPlaces(targetPoses, actions);
    serverGameState.characters = targetPoses;
  }

  void CancelMovementForCounterRolesSwapingPlaces(TeamRoleMap<Character> targetPoses, List<TurnAction> actions)
  {
    if (AreSwapingPlaces(targetPoses, Team.Red, Role.Worm, Team.Blue, Role.Planter))
      targetPoses.ReplaceWithItemFrom(serverGameState.characters, Team.Red, Role.Worm);
    else if (AreSwapingPlaces(targetPoses, Team.Red, Role.Worm, Team.Blue, Role.Harvester))
      targetPoses.ReplaceWithItemFrom(serverGameState.characters, Team.Blue, Role.Harvester);

    if (AreSwapingPlaces(targetPoses, Team.Blue, Role.Worm, Team.Red, Role.Planter))
      targetPoses.ReplaceWithItemFrom(serverGameState.characters, Team.Blue, Role.Worm);
    else if (AreSwapingPlaces(targetPoses, Team.Blue, Role.Worm, Team.Red, Role.Harvester))
      targetPoses.ReplaceWithItemFrom(serverGameState.characters, Team.Red, Role.Harvester);
  }

  bool AreSwapingPlaces(TeamRoleMap<Character> targetPoses, Team char1Team, Role char1Role, Team char2Team, Role char2Role)
  {
    return targetPoses.GetItem(char1Team, char1Role).DistanceTo(serverGameState.characters.GetItem(char2Team, char2Role)) == 0
          && targetPoses.GetItem(char2Team, char2Role).DistanceTo(serverGameState.characters.GetItem(char1Team, char1Role)) == 0;
  }

  void DoCatchWorm(ServerGameState serverGameState)
  {
    foreach (var planter in serverGameState.characters.GetItemsBy(Role.Planter))
    {
      foreach (var worm in serverGameState.characters.GetItemsBy(Role.Worm))
      {
        if (planter.team == worm.team) continue;
        if (worm.DistanceTo(planter) != 0) continue;

        var newWormState = worm.Clone();
        var newPlanterState = planter.Clone();

        var wormNewPosition = mapInfo.startingPositions.GetItem(worm.team, worm.role);
        newWormState.x = (int)wormNewPosition.X;
        newWormState.y = (int)wormNewPosition.Y;
        serverGameState.characters.SetItem(worm.team, worm.role, newWormState);

        newPlanterState.numWormCaught++;
        serverGameState.characters.SetItem(planter.team, planter.role, newPlanterState);
      }
    }
  }

  void DoScareHarvester(ServerGameState serverGameState)
  {
    foreach (var worm in serverGameState.characters.GetItemsBy(Role.Worm))
    {
      foreach (var harvester in serverGameState.characters.GetItemsBy(Role.Harvester))
      {
        if (worm.team == harvester.team) continue;
        if (harvester.DistanceTo(worm) != 0) continue;

        var newHarvesterState = harvester.Clone();
        var newWormState = worm.Clone();

        newHarvesterState.isScared = true;
        newHarvesterState.fruitCarrying = 0;
        serverGameState.characters.SetItem(harvester.team, harvester.role, newHarvesterState);

        newWormState.numHarvesterScared++;
        serverGameState.characters.SetItem(worm.team, worm.role, newWormState);
      }
    }
  }

  void DoPlantTree(ServerGameState serverGameState)
  {
    foreach (var planter in serverGameState.characters.GetItemsBy(Role.Planter))
    {
      var currentTile = serverGameState.map[planter.x][planter.y];
      if (currentTile.type == TileType.EMPTY)
      {
        currentTile.type = gameRule.FruitTileTypeForTeam(planter.team);
        serverGameState.map[planter.x][planter.y] = currentTile;

        var newPlanterState = planter.Clone();
        newPlanterState.numTreePlanted++;
        serverGameState.characters.SetItem(planter.team, planter.role, newPlanterState);
      }
    }
  }

  void DoHarvest(ServerGameState serverGameState)
  {
    var harvesters = serverGameState.characters.GetItemsBy(Role.Harvester);
    foreach (var harvester in harvesters)
    {
      if (harvester.fruitCarrying >= gameRule.harvesterMaxCapacity || harvester.isScared) continue;

      var currentTile = serverGameState.map[harvester.x][harvester.y];
      var teamFruitType = gameRule.FruitTileTypeForTeam(harvester.team);

      if (IsTileRipeHarvestableFruit(currentTile, teamFruitType))
      {
        currentTile.growState = 1;
        serverGameState.map[harvester.x][harvester.y] = currentTile;

        var harvestersSharingFruit = new List<Character>() { harvester.Clone() };
        var fruitValue = gameRule.fruitScoreValues[currentTile.type];

        if (currentTile.type == TileType.WILDBERRY)
        {
          harvestersSharingFruit.AddRange(harvesters.Where(h => h.team != harvester.team && !h.isScared && h.x == harvester.x && h.y == harvester.y).Select(h => h.Clone()));
          if (harvestersSharingFruit.Count > 1) fruitValue /= 2;
        }

        foreach (var harvestingHarvester in harvestersSharingFruit)
        {
          harvestingHarvester.fruitCarrying += fruitValue;
          harvestingHarvester.numFruitHarvested += fruitValue;
          serverGameState.characters.SetItem(harvestingHarvester.team, harvestingHarvester.role, harvestingHarvester);
        }
      }
    }
  }

  bool IsTileRipeHarvestableFruit(Tile tile, TileType teamFruitType)
  {
    return (tile.type == teamFruitType && tile.growState >= gameRule.plantFruitTime)
      || (tile.type == TileType.WILDBERRY && tile.growState >= gameRule.wildberryFruitTime);
  }

  void DoGetPoint(ServerGameState serverGameState)
  {
    foreach (var harvester in serverGameState.characters.GetItemsBy(Role.Harvester))
    {
      var currentTile = serverGameState.map[harvester.x][harvester.y];
      var scoreTileType = gameRule.ScoreTileTypeForTeam(harvester.team);

      if (currentTile.type == scoreTileType)
      {
        if (harvester.team == Team.Red) serverGameState.redScore += harvester.fruitCarrying;
        else serverGameState.blueScore += harvester.fruitCarrying;

        var newHarvesterState = harvester.Clone();
        newHarvesterState.numFruitDelivered += harvester.fruitCarrying;
        newHarvesterState.fruitCarrying = 0;
        serverGameState.characters.SetItem(harvester.team, harvester.role, newHarvesterState);
      }
    }
  }

  void DoDestroyPlant(ServerGameState serverGameState)
  {
    foreach (var worm in serverGameState.characters.GetItemsBy(Role.Worm))
    {
      var currentTile = serverGameState.map[worm.x][worm.y];
      var destroyablePlantTypes = gameRule.WormDestroyTileTypeForTeam(worm.team);
      if (destroyablePlantTypes.Contains(currentTile.type))
      {
        currentTile.type = TileType.EMPTY;
        currentTile.growState = 0;
        serverGameState.map[worm.x][worm.y] = currentTile;

        var newWormState = worm.Clone();
        newWormState.numTreeDestroyed++;
        serverGameState.characters.SetItem(worm.team, worm.role, newWormState);
      }
    }
  }

  void DoGrowPlant(ServerGameState serverGameState)
  {
    for (int row = 0; row < serverGameState.map.Count; row++)
    {
      for (int col = 0; col < serverGameState.map[row].Count; col++)
      {
        var tile = serverGameState.map[row][col];

        if (IsUnripePlant(tile))
        {
          tile.growState++;
          serverGameState.map[row][col] = tile;
        }
      }
    }
  }

  bool IsUnripePlant(Tile tile)
  {
    return (tile.type == TileType.WILDBERRY && tile.growState < gameRule.wildberryFruitTime)
    || ((tile.type == TileType.PUMPKIN || tile.type == TileType.TOMATO) && tile.growState < gameRule.plantFruitTime);
  }

  bool IsInImpassableTile(ServerGameState serverGameState, Character character)
  {
    return serverGameState.map[character.x][character.y].type == TileType.IMPASSABLE;
  }

  ServerGameState InitializeCharacters(ServerGameState gameState, MapInfo mapInfo)
  {
    foreach (var team in gameRule.availableTeams)
    {
      foreach (var role in gameRule.availableRoles)
      {
        var pos = mapInfo.startingPositions.GetItem(team, role);
        Character character = new Character((int)pos.X, (int)pos.Y, team, role);
        gameState.characters.SetItem(team, role, character);
      }
    }
    return gameState;
  }

  void AssignCharacterToControllers(ServerGameState gameState, TeamRoleMap<ICharacterDescisionMaker> controllers)
  {
    foreach (var character in gameState.characters)
    {
      controllers.GetItem(character.team, character.role).Character = character;
    }
  }

  ServerGameState InitializeMap(ServerGameState gameState, MapInfo mapInfo)
  {
    for (int x = 0; x < mapInfo.tiles.Count; x++)
    {
      gameState.map.Add(new List<Tile>());
      for (int y = 0; y < mapInfo.tiles[x].Count; y++)
      {
        var tile = new Tile(x, y);
        tile.type = mapInfo.tiles[x][y];

        if (IsTileTypeAlwaysVisible(tile.type)) tile.alwaysVisible = true;

        if (tile.type == TileType.WILDBERRY)
        {
          tile.growState = gameRule.wildberryFruitTime;
        }

        gameState.map[x].Add(tile);
      }
    }
    return gameState;
  }

  bool IsTileTypeAlwaysVisible(TileType type) => type == TileType.RED_BOX || type == TileType.BLUE_BOX || type == TileType.RED_ROCK || type == TileType.BLUE_ROCK;
}