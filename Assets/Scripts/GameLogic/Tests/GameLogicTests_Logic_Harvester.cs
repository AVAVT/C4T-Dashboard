using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Numerics;
using System;

namespace GameLogicTests
{
  public class GameLogicTests_Logic_Harvester
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
    public void Harvester_Cant_Harvest_When_Scared()
    {
      var customMapInfo = CreateTestMapInfo();
      customMapInfo.startingPositions.SetItem(Team.Red, Role.Harvester, new Vector2(4, 0));
      customMapInfo.startingPositions.SetItem(Team.Blue, Role.Worm, new Vector2(4, 1));
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, customMapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.DOWN),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };
      gameLogic.ExecuteTurn(actions);

      var gameState = gameLogic.GetGameStateSnapShot();
      var characters = gameState.characters;
      var tile = gameState.map[4][1];
      Assert.IsTrue(characters.GetItem(Team.Red, Role.Harvester).isScared);
      Assert.AreEqual(0, characters.GetItem(Team.Red, Role.Harvester).fruitCarrying);
      Assert.AreEqual(TileType.WILDBERRY, tile.type);
      Assert.AreEqual(gameConfig.wildberryFruitTime, tile.growState);
    }

    [Test]
    public void Harvester_Lose_Fruit_Carrying_When_Scared()
    {
      var customMapInfo = CreateTestMapInfo();
      customMapInfo.startingPositions.SetItem(Team.Red, Role.Harvester, new Vector2(4, 1));
      customMapInfo.startingPositions.SetItem(Team.Blue, Role.Worm, new Vector2(4, 0));
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
      actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.DOWN)
      };
      gameLogic.ExecuteTurn(actions);

      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(0, characters.GetItem(Team.Red, Role.Harvester).fruitCarrying);
    }

    [Test]
    public void Harvester_Harvest_Team_Plant()
    {
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, mapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.RIGHT),
        new TurnAction(Team.Red, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };

      for (int i = 0; i < gameConfig.plantFruitTime; i++)
      {
        gameLogic.ExecuteTurn(actions);
      }
      actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.RIGHT),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };

      gameLogic.ExecuteTurn(actions);

      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(1, characters.GetItem(Team.Red, Role.Harvester).fruitCarrying);
    }

    [Test]
    public void Harvester_Harvest_Wildberry()
    {
      var customMapInfo = CreateTestMapInfo();
      customMapInfo.startingPositions.SetItem(Team.Red, Role.Harvester, new Vector2(4, 0));
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, customMapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.DOWN),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };

      gameLogic.ExecuteTurn(actions);

      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(2, characters.GetItem(Team.Red, Role.Harvester).fruitCarrying);
    }

    [Test]
    public void Harvester_Harvest_Shared_Wildberry()
    {
      var customMapInfo = CreateTestMapInfo();
      customMapInfo.startingPositions.SetItem(Team.Red, Role.Harvester, new Vector2(4, 0));
      customMapInfo.startingPositions.SetItem(Team.Blue, Role.Harvester, new Vector2(4, 2));
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, customMapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.DOWN),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.UP),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };

      gameLogic.ExecuteTurn(actions);

      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(1, characters.GetItem(Team.Red, Role.Harvester).fruitCarrying);
      Assert.AreEqual(1, characters.GetItem(Team.Blue, Role.Harvester).fruitCarrying);
    }

    [Test]
    public void Harvester_Reset_GrowState_When_Harvested()
    {
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, mapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.RIGHT),
        new TurnAction(Team.Red, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };

      for (int i = 0; i < gameConfig.plantFruitTime; i++)
      {
        gameLogic.ExecuteTurn(actions);
      }
      actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.RIGHT),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };

      gameLogic.ExecuteTurn(actions);

      var tile = gameLogic.GetGameStateSnapShot().map[1][0];
      Assert.AreEqual(TileType.TOMATO, tile.type);
      Assert.AreEqual(1, tile.growState);
    }

    [Test]
    public void Harvester_Record_numFruitHarvested_Correctly()
    {
      var customMapInfo = CreateTestMapInfo();
      customMapInfo.startingPositions.SetItem(Team.Red, Role.Harvester, new Vector2(4, 0));
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, customMapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.DOWN),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };

      gameLogic.ExecuteTurn(actions);

      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(2, characters.GetItem(Team.Red, Role.Harvester).numFruitHarvested);
    }

    [Test]
    public void Harvester_Score_Point()
    {
      var customMapInfo = CreateTestMapInfo();
      customMapInfo.startingPositions.SetItem(Team.Red, Role.Harvester, new Vector2(4, 0));
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, customMapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.DOWN),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };
      gameLogic.ExecuteTurn(actions);

      actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.UP),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };
      gameLogic.ExecuteTurn(actions);
      actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.LEFT),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };
      gameLogic.ExecuteTurn(actions);
      gameLogic.ExecuteTurn(actions);
      gameLogic.ExecuteTurn(actions);
      gameLogic.ExecuteTurn(actions);

      var gameState = gameLogic.GetGameStateSnapShot();
      var characters = gameState.characters;
      Assert.AreEqual(0, characters.GetItem(Team.Red, Role.Harvester).fruitCarrying);
      Assert.AreEqual(2, gameState.redScore);
    }

    [Test]
    public void Harvester_Record_numFruitDelivered_Correctly()
    {
      var customMapInfo = CreateTestMapInfo();
      customMapInfo.startingPositions.SetItem(Team.Red, Role.Harvester, new Vector2(4, 0));
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, customMapInfo);
      List<TurnAction> actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.DOWN),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };
      gameLogic.ExecuteTurn(actions);

      actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.UP),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };
      gameLogic.ExecuteTurn(actions);
      actions = new List<TurnAction>()
      {
        new TurnAction(Team.Red, Role.Planter, Directions.STAY),
        new TurnAction(Team.Red, Role.Harvester, Directions.LEFT),
        new TurnAction(Team.Red, Role.Worm, Directions.STAY),
        new TurnAction(Team.Blue, Role.Planter, Directions.STAY),
        new TurnAction(Team.Blue, Role.Harvester, Directions.STAY),
        new TurnAction(Team.Blue, Role.Worm, Directions.STAY)
      };
      gameLogic.ExecuteTurn(actions);
      gameLogic.ExecuteTurn(actions);
      gameLogic.ExecuteTurn(actions);
      gameLogic.ExecuteTurn(actions);

      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(2, characters.GetItem(Team.Red, Role.Harvester).numFruitDelivered);
    }
  }
}
