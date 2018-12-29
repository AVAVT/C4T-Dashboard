using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Numerics;
using System;

namespace GameLogicTests
{
  public class GameLogicTests_Logic_Plant
  {
    GameConfig gameConfig;
    MapInfo mapInfo;

    [SetUp]
    public void Setup()
    {
      gameConfig = GameConfig.DefaultGameRule();
      mapInfo = CreateTestMapInfo();
    }

    MapInfo CreateTestMapInfo()
    {
      var result = new MapInfo();

      var startingPositions = new TeamRoleMap<Vector2>();
      startingPositions.SetItem(Team.Red, Role.Planter, new Vector2(0, 0));
      startingPositions.SetItem(Team.Red, Role.Harvester, new Vector2(0, 0));
      startingPositions.SetItem(Team.Red, Role.Worm, new Vector2(0, 4));
      startingPositions.SetItem(Team.Blue, Role.Planter, new Vector2(6, 4));
      startingPositions.SetItem(Team.Blue, Role.Harvester, new Vector2(6, 4));
      startingPositions.SetItem(Team.Blue, Role.Worm, new Vector2(6, 0));
      result.startingPositions = startingPositions;

      var tiles = new List<List<TileType>>()
      {
        new List<TileType>(){TileType.RED_BOX, TileType.EMPTY, TileType.EMPTY , TileType.EMPTY , TileType.RED_ROCK },
        new List<TileType>(){TileType.EMPTY, TileType.EMPTY, TileType.EMPTY , TileType.EMPTY , TileType.EMPTY },
        new List<TileType>(){TileType.EMPTY, TileType.IMPASSABLE, TileType.EMPTY , TileType.WILDBERRY , TileType.EMPTY },
        new List<TileType>(){TileType.EMPTY, TileType.EMPTY, TileType.EMPTY , TileType.EMPTY , TileType.EMPTY },
        new List<TileType>(){TileType.EMPTY, TileType.WILDBERRY, TileType.EMPTY , TileType.IMPASSABLE , TileType.EMPTY },
        new List<TileType>(){TileType.EMPTY, TileType.EMPTY, TileType.EMPTY , TileType.EMPTY , TileType.EMPTY },
        new List<TileType>(){TileType.BLUE_ROCK, TileType.EMPTY, TileType.EMPTY , TileType.EMPTY , TileType.BLUE_BOX }
      };
      result.tiles = tiles;

      return result;
    }

    [Test]
    public void Plant_Increase_GrowState()
    {
      var customMapInfo = CreateTestMapInfo();
      customMapInfo.startingPositions.SetItem(Team.Red, Role.Planter, new Vector2(1, 0));
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, customMapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };
      gameLogic.ExecuteTurn(actions);
      gameLogic.ExecuteTurn(actions);

      var tile = gameLogic.GetGameStateSnapShot().map[1][0];
      Assert.AreEqual(2, tile.growState);
    }

    [Test]
    public void Plant_Doesnt_Increase_GrowState_At_Max()
    {
      var customMapInfo = CreateTestMapInfo();
      customMapInfo.startingPositions.SetItem(Team.Red, Role.Planter, new Vector2(1, 0));
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, customMapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };

      for (int i = 0; i < gameConfig.plantFruitTime * 2; i++) gameLogic.ExecuteTurn(actions);

      var tile = gameLogic.GetGameStateSnapShot().map[1][0];
      Assert.AreEqual(gameConfig.plantFruitTime, tile.growState);
    }

    [Test]
    public void Wildberry_Increase_GrowState()
    {
      var customMapInfo = CreateTestMapInfo();
      customMapInfo.startingPositions.SetItem(Team.Red, Role.Harvester, new Vector2(4, 1));
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, customMapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };
      gameLogic.ExecuteTurn(actions);
      gameLogic.ExecuteTurn(actions);
      gameLogic.ExecuteTurn(actions);

      var tile = gameLogic.GetGameStateSnapShot().map[4][1];
      Assert.AreEqual(3, tile.growState);
    }

    [Test]
    public void Wildberry_Doesnt_Increase_GrowState_At_Max()
    {
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };

      for (int i = 0; i < gameConfig.wildberryFruitTime * 2; i++) gameLogic.ExecuteTurn(actions);

      var tile = gameLogic.GetGameStateSnapShot().map[4][1];
      Assert.AreEqual(gameConfig.wildberryFruitTime, tile.growState);
    }
  }
}
