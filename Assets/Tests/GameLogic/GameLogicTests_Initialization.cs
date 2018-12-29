using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Numerics;

namespace GameLogicTests
{
  public class GameLogicTests_Initialization
  {
    GameConfig gameConfig;
    MapInfo mapInfo;

    [OneTimeSetUp]
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
    public void ServerGameState_Initialized_Not_NULL()
    {
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      Assert.IsNotNull(gameLogic.GetGameStateSnapShot());
    }

    [Test]
    public void ServerGameState_Initialized_Correct_Size()
    {
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var map = gameLogic.GetGameStateSnapShot().map;
      Assert.AreEqual(mapInfo.tiles.Count, map.Count);
      Assert.AreEqual(mapInfo.tiles[0].Count, map[0].Count);
    }

    [Test]
    public void ServerGameState_Initialized_RedPlanter_Position()
    {
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(0, characters.GetItem(Team.Red, Role.Planter).x);
      Assert.AreEqual(0, characters.GetItem(Team.Red, Role.Planter).y);
    }

    [Test]
    public void ServerGameState_Initialized_RedHarvester_Position()
    {
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(0, characters.GetItem(Team.Red, Role.Harvester).x);
      Assert.AreEqual(0, characters.GetItem(Team.Red, Role.Harvester).y);
    }

    [Test]
    public void ServerGameState_Initialized_RedWorm_Position()
    {
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(0, characters.GetItem(Team.Red, Role.Worm).x);
      Assert.AreEqual(4, characters.GetItem(Team.Red, Role.Worm).y);
    }

    [Test]
    public void ServerGameState_Initialized_BluePlanter_Position()
    {
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(6, characters.GetItem(Team.Blue, Role.Planter).x);
      Assert.AreEqual(4, characters.GetItem(Team.Blue, Role.Planter).y);
    }

    [Test]
    public void ServerGameState_Initialized_BlueHarvester_Position()
    {
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(6, characters.GetItem(Team.Blue, Role.Harvester).x);
      Assert.AreEqual(4, characters.GetItem(Team.Blue, Role.Harvester).y);
    }

    [Test]
    public void ServerGameState_Initialized_BlueWorm_Position()
    {
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var characters = gameLogic.GetGameStateSnapShot().characters;
      Assert.AreEqual(6, characters.GetItem(Team.Blue, Role.Worm).x);
      Assert.AreEqual(0, characters.GetItem(Team.Blue, Role.Worm).y);
    }

    [Test]
    public void ServerGameState_Initialized_WildBerry_Position()
    {
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var map = gameLogic.GetGameStateSnapShot().map;
      Assert.AreEqual(TileType.WILDBERRY, map[2][3].type);
      Assert.AreEqual(TileType.WILDBERRY, map[4][1].type);
    }
    [Test]
    public void ServerGameState_Initialized_WildBerry_GrowState()
    {
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var map = gameLogic.GetGameStateSnapShot().map;
      Assert.AreEqual(gameConfig.wildberryFruitTime, map[2][3].growState);
      Assert.AreEqual(gameConfig.wildberryFruitTime, map[4][1].growState);
    }

    [Test]
    public void ServerGameState_Initialized_Water_Position()
    {
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var map = gameLogic.GetGameStateSnapShot().map;
      Assert.AreEqual(TileType.IMPASSABLE, map[2][1].type);
      Assert.AreEqual(TileType.IMPASSABLE, map[4][3].type);
    }

    [Test]
    public void ServerGameState_Initialized_Empty_GrowState()
    {
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var map = gameLogic.GetGameStateSnapShot().map;
      foreach (var col in map)
      {
        foreach (var tile in col)
        {
          if (tile.type == TileType.EMPTY) Assert.AreEqual(0, tile.growState);
        }
      }
    }
  }
}
