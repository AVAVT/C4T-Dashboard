using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Numerics;
using System;

namespace GameLogicTests
{
  public class GameLogicTests_Logic_AllCharacters
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
    public void Characters_Move_Correctly()
    {
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, mapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.DOWN),
        new TurnAction(Team.Red, Role.Harvester, Directions.DOWN),
        new TurnAction(Team.Red, Role.Worm, Directions.RIGHT),
        new TurnAction(Team.Blue, Role.Planter, Directions.UP),
        new TurnAction(Team.Blue, Role.Harvester, Directions.UP),
        new TurnAction(Team.Blue, Role.Worm, Directions.LEFT)
      };
      gameLogic.ExecuteTurn(actions);

      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(0, characters.GetItem(Team.Red, Role.Planter).x);
      Assert.AreEqual(1, characters.GetItem(Team.Red, Role.Planter).y);
      Assert.AreEqual(0, characters.GetItem(Team.Red, Role.Harvester).x);
      Assert.AreEqual(1, characters.GetItem(Team.Red, Role.Harvester).y);
      Assert.AreEqual(1, characters.GetItem(Team.Red, Role.Worm).x);
      Assert.AreEqual(4, characters.GetItem(Team.Red, Role.Worm).y);

      Assert.AreEqual(6, characters.GetItem(Team.Blue, Role.Planter).x);
      Assert.AreEqual(3, characters.GetItem(Team.Blue, Role.Planter).y);
      Assert.AreEqual(6, characters.GetItem(Team.Blue, Role.Harvester).x);
      Assert.AreEqual(3, characters.GetItem(Team.Blue, Role.Harvester).y);
      Assert.AreEqual(5, characters.GetItem(Team.Blue, Role.Worm).x);
      Assert.AreEqual(0, characters.GetItem(Team.Blue, Role.Worm).y);
    }

    [Test]
    public void Characters_Cant_Move_Out_Of_Map()
    {
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, mapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.DOWN),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };
      gameLogic.ExecuteTurn(actions);

      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(6, characters.GetItem(Team.Blue, Role.Harvester).x);
      Assert.AreEqual(4, characters.GetItem(Team.Blue, Role.Harvester).y);
    }

    [Test]
    public void Characters_Cant_Move_Into_Water()
    {
      var customMapInfo = CreateTestMapInfo();
      customMapInfo.startingPositions.SetItem(Team.Red, Role.Planter, new Vector2(2, 0));
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, customMapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.DOWN),
        new TurnAction(Team.Red, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };
      gameLogic.ExecuteTurn(actions);

      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(2, characters.GetItem(Team.Red, Role.Planter).x);
      Assert.AreEqual(0, characters.GetItem(Team.Red, Role.Planter).y);
    }

    [Test]
    public void Characters_Stay_Correctly()
    {
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, mapInfo);
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

      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(0, characters.GetItem(Team.Red, Role.Planter).x);
      Assert.AreEqual(0, characters.GetItem(Team.Red, Role.Planter).y);
      Assert.AreEqual(0, characters.GetItem(Team.Red, Role.Harvester).x);
      Assert.AreEqual(0, characters.GetItem(Team.Red, Role.Harvester).y);
      Assert.AreEqual(0, characters.GetItem(Team.Red, Role.Worm).x);
      Assert.AreEqual(4, characters.GetItem(Team.Red, Role.Worm).y);

      Assert.AreEqual(6, characters.GetItem(Team.Blue, Role.Planter).x);
      Assert.AreEqual(4, characters.GetItem(Team.Blue, Role.Planter).y);
      Assert.AreEqual(6, characters.GetItem(Team.Blue, Role.Harvester).x);
      Assert.AreEqual(4, characters.GetItem(Team.Blue, Role.Harvester).y);
      Assert.AreEqual(6, characters.GetItem(Team.Blue, Role.Worm).x);
      Assert.AreEqual(0, characters.GetItem(Team.Blue, Role.Worm).y);
    }
  }
}
