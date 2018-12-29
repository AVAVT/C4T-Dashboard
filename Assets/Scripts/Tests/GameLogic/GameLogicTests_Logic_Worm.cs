using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Numerics;
using System;

namespace GameLogicTests
{
  public class GameLogicTests_Logic_Worm
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
    public void Worm_Cant_Destroy_Plant_When_Caught()
    {
      var customMapInfo = CreateTestMapInfo();
      customMapInfo.startingPositions.SetItem(Team.Red, Role.Planter, new Vector2(4, 0));
      customMapInfo.startingPositions.SetItem(Team.Blue, Role.Worm, new Vector2(5, 0));
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, customMapInfo);
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

      actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.LEFT)
      };
      gameLogic.ExecuteTurn(actions);

      var gameState = gameLogic.GetGameStateSnapShot();
      var characters = gameState.characters;
      var tile = gameState.map[4][0];
      Assert.AreEqual(5, characters.GetItem(Team.Blue, Role.Worm).x);
      Assert.AreEqual(0, characters.GetItem(Team.Blue, Role.Worm).y);
      Assert.AreEqual(TileType.TOMATO, tile.type);
      Assert.AreEqual(Math.Min(4, gameConfig.plantFruitTime), tile.growState);
    }

    [Test]
    public void Worm_Cant_Scare_Harvester_When_Caught()
    {
      var customMapInfo = CreateTestMapInfo();
      customMapInfo.startingPositions.SetItem(Team.Red, Role.Planter, new Vector2(4, 0));
      customMapInfo.startingPositions.SetItem(Team.Red, Role.Harvester, new Vector2(4, 0));
      customMapInfo.startingPositions.SetItem(Team.Blue, Role.Worm, new Vector2(5, 0));
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, customMapInfo);

      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.LEFT)
      };
      gameLogic.ExecuteTurn(actions);

      actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.LEFT)
      };
      gameLogic.ExecuteTurn(actions);

      var gameState = gameLogic.GetGameStateSnapShot();
      var characters = gameState.characters;
      var tile = gameState.map[4][0];
      Assert.AreEqual(5, characters.GetItem(Team.Blue, Role.Worm).x);
      Assert.AreEqual(0, characters.GetItem(Team.Blue, Role.Worm).y);
      Assert.IsFalse(characters.GetItem(Team.Red, Role.Harvester).isScared);
    }

    [Test]
    public void Worm_Scare_Harvester()
    {
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, mapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.RIGHT),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.LEFT)
      };
      gameLogic.ExecuteTurn(actions);
      gameLogic.ExecuteTurn(actions);
      gameLogic.ExecuteTurn(actions);

      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.IsTrue(characters.GetItem(Team.Red, Role.Harvester).isScared);
    }

    [Test]
    public void Worm_Record_numHarvesterScared_Correctly()
    {
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, mapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.RIGHT),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.LEFT)
      };
      gameLogic.ExecuteTurn(actions);
      gameLogic.ExecuteTurn(actions);
      gameLogic.ExecuteTurn(actions);

      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(1, characters.GetItem(Team.Blue, Role.Worm).numHarvesterScared);
    }

    [Test]
    public void Worm_Destroy_Plant()
    {
      var customMapInfo = CreateTestMapInfo();
      customMapInfo.startingPositions.SetItem(Team.Red, Role.Planter, new Vector2(5, 0));
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, customMapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.LEFT),
        new TurnAction(Team.Red, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.LEFT)
      };
      gameLogic.ExecuteTurn(actions);
      gameLogic.ExecuteTurn(actions);

      var tile = gameLogic.GetGameStateSnapShot().map[5][0];
      Assert.AreEqual(TileType.EMPTY, tile.type);
      Assert.AreEqual(0, tile.growState);
    }

    [Test]
    public void Worm_Record_numTreeDestroyed_Correctly()
    {
      var customMapInfo = CreateTestMapInfo();
      customMapInfo.startingPositions.SetItem(Team.Red, Role.Planter, new Vector2(5, 0));
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, customMapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.LEFT),
        new TurnAction(Team.Red, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.LEFT)
      };
      gameLogic.ExecuteTurn(actions);
      gameLogic.ExecuteTurn(actions);

      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(1, characters.GetItem(Team.Blue, Role.Worm).numTreeDestroyed);
    }
  }
}
