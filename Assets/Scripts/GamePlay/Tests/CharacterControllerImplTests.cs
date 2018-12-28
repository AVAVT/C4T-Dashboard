using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace GameplayTests
{
  public class CharacterControllerImplTests
  {
    private class MockRecorder : IReplayRecorder
    {
      public int turnNumber = 0;
      public void LogEndGame(ServerGameState serverGameState)
      {
        turnNumber = serverGameState.turn;
      }

      public void LogGameStart(GameConfig gameRule, ServerGameState serverGameState)
      {
        turnNumber = serverGameState.turn;
      }

      public void LogTurn(ServerGameState serverGameState, List<TurnAction> actions)
      {
        turnNumber = serverGameState.turn;
      }
    }
    IEnumerator LoadTextMapTexture(Action<Texture2D> callback)
    {
      var www = UnityWebRequestTexture.GetTexture("file://" + System.IO.Path.Combine(Application.streamingAssetsPath, "MapTextures/11x9.png"));
      yield return www.SendWebRequest();
      callback(((DownloadHandlerTexture)www.downloadHandler).texture);
    }

    [UnityTest]
    public IEnumerator DoStart_Successfully()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);
      var gameConfig = GameConfig.DefaultGameRule();
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, mapInfo);

      var controller = new WebServiceCharacterController();
      var task = controller.DoStart(gameLogic.GetGameStateSnapShot().GameStateForTeam(Team.Red, gameConfig), gameConfig);
      yield return new WaitUntil(() => task.IsCompleted);
      Assert.IsFalse(controller.IsCrashed);
      Assert.IsFalse(controller.IsTimedOut);
    }

    [UnityTest]
    public IEnumerator DoTurn_Successfully()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);
      var gameConfig = GameConfig.DefaultGameRule();
      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, mapInfo);

      var controller = new WebServiceCharacterController();
      var task = controller.DoTurn(gameLogic.GetGameStateSnapShot().GameStateForTeam(Team.Red, gameConfig), gameConfig);
      yield return new WaitUntil(() => task.IsCompleted);
      Assert.IsFalse(controller.IsCrashed);
      Assert.IsFalse(controller.IsTimedOut);
    }

    [UnityTest]
    public IEnumerator CharacterControllerImplTestsWorkUntilLastTurn()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);
      var gameConfig = new GameConfig(
        sightDistance: 2,
        gameLength: 4,
        plantFruitTime: 5,
        wildberryFruitTime: 10,
        fruitHarvestValue: 1,
        wildberryHarvestValue: 2,
        harvesterMaxCapacity: 5,
        availableTeams: new List<Team>() { Team.Red, Team.Blue },
        availableRoles: new List<Role>() { Role.Planter, Role.Harvester, Role.Worm }
      );

      var gameLogic = GameLogic.GameLogicForPlay(gameConfig, mapInfo);

      var controllerMap = new TeamRoleMap<ICharacterController>();
      controllerMap.SetItem(Team.Red, Role.Planter, new WebServiceCharacterController());
      controllerMap.SetItem(Team.Red, Role.Harvester, new WebServiceCharacterController());
      controllerMap.SetItem(Team.Red, Role.Worm, new WebServiceCharacterController());
      controllerMap.SetItem(Team.Blue, Role.Planter, new WebServiceCharacterController());
      controllerMap.SetItem(Team.Blue, Role.Harvester, new WebServiceCharacterController());
      controllerMap.SetItem(Team.Blue, Role.Worm, new WebServiceCharacterController());

      gameLogic.InitializeGame(controllerMap);
      var mockRecorder = new MockRecorder();

      var task = gameLogic.PlayGame(mockRecorder);
      yield return new WaitUntil(() => task.IsCompleted);
      Assert.AreEqual(gameConfig.gameLength, mockRecorder.turnNumber);
    }
  }
}
