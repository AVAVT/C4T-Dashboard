using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace AIPlayTests
{
  public class CharacterControllerImplTests
  {
    System.Diagnostics.Process process;

    [OneTimeSetUp]
    public void SetupMockServer()
    {
      process = new System.Diagnostics.Process();
      process.StartInfo.FileName = "node.exe";
      process.StartInfo.Arguments = $"\"{Application.dataPath}/Tests/MockAIServer-JS/index.js\"";
      process.Start();
    }

    [OneTimeTearDown]
    public void StopMockServer()
    {
      process.Kill();
    }

    [UnityTest]
    [Timeout(100000000)]
    public IEnumerator BruteCallTest()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);
      var gameConfig = GameConfig.DefaultGameRule();
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var controller = new WebServiceDecisionMaker("http://localhost:8686");
      controller.Character = new Character(0, 0, Team.Red, Role.Harvester);

      int i = 0;
      var gameState = gameLogic.GetGameStateSnapShot().GameStateForTeam(Team.Red, gameConfig);
      while (i < 1000)
      {
        i++;
        gameState.turn = i;
        var task = controller.DoTurn(gameState, gameConfig);
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsCanceled || task.IsFaulted)
        {
          Debug.LogError(task.Exception);
          break;
        }
      }
      Assert.AreEqual(i, 1000);
    }

    [UnityTest]
    public IEnumerator DoStart_Successfully()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);
      var gameConfig = GameConfig.DefaultGameRule();
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var controller = CreateTestController("/start", "/turn");

      var task = controller.DoTurn(gameLogic.GetGameStateSnapShot().GameStateForTeam(Team.Red, gameConfig), gameConfig);
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
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var controller = CreateTestController("/start", "/turn");

      var task = controller.DoTurn(gameLogic.GetGameStateSnapShot().GameStateForTeam(Team.Red, gameConfig), gameConfig);
      yield return new WaitUntil(() => task.IsCompleted);
      Assert.AreEqual(Directions.DOWN, task.Result);
      Assert.IsFalse(controller.IsCrashed);
      Assert.IsFalse(controller.IsTimedOut);
    }

    [UnityTest]
    public IEnumerator Handle_Start_Timeout_Correctly()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);
      var gameConfig = GameConfig.DefaultGameRule();
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var controller = CreateTestController("/timeout", "/timeout");

      var task = controller.DoStart(gameLogic.GetGameStateSnapShot().GameStateForTeam(Team.Red, gameConfig), gameConfig);
      yield return new WaitUntil(() => task.IsCompleted);

      Assert.IsFalse(controller.IsCrashed);
      Assert.IsTrue(controller.IsTimedOut);
    }

    [UnityTest]
    public IEnumerator Handle_Turn_Timeout_Correctly()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);
      var gameConfig = GameConfig.DefaultGameRule();
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var controller = CreateTestController("/timeout", "/timeout");

      var task = controller.DoTurn(gameLogic.GetGameStateSnapShot().GameStateForTeam(Team.Red, gameConfig), gameConfig);
      yield return new WaitUntil(() => task.IsCompleted);

      Assert.IsFalse(controller.IsCrashed);
      Assert.IsTrue(controller.IsTimedOut);
    }

    [UnityTest]
    public IEnumerator Handle_Start_Crash_Correctly()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);
      var gameConfig = GameConfig.DefaultGameRule();
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var controller = CreateTestController("/crash", "/crash");

      var task = controller.DoStart(gameLogic.GetGameStateSnapShot().GameStateForTeam(Team.Red, gameConfig), gameConfig);
      yield return new WaitUntil(() => task.IsCompleted);

      Assert.IsTrue(controller.IsCrashed);
      Assert.IsFalse(controller.IsTimedOut);
    }

    [UnityTest]
    public IEnumerator Handle_Turn_Crash_Correctly()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);
      var gameConfig = GameConfig.DefaultGameRule();
      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);
      var controller = CreateTestController("/crash", "/crash");

      var task = controller.DoTurn(gameLogic.GetGameStateSnapShot().GameStateForTeam(Team.Red, gameConfig), gameConfig);
      yield return new WaitUntil(() => task.IsCompleted);

      Assert.IsTrue(controller.IsCrashed);
      Assert.IsFalse(controller.IsTimedOut);
    }

    [UnityTest]
    public IEnumerator Handshake_Successfully()
    {

      var controller = CreateTestController();

      var task = controller.Handshake(0, 0);
      yield return new WaitUntil(() => task.IsCompleted);

      Assert.IsTrue(controller.IsReady);
      Assert.AreEqual("Test Player Name", controller.PlayerName);
    }

    [UnityTest]
    public IEnumerator Handle_Handshake_Crash_Correctly()
    {
      var controller = CreateTestController("/start", "/turn", "/namecrash");

      var task = controller.Handshake(0, 0);
      yield return new WaitUntil(() => task.IsCompleted);

      Assert.IsFalse(controller.IsReady);
      Assert.AreEqual("UNKNOWN", controller.PlayerName);
    }

    [UnityTest]
    public IEnumerator Work_Until_Last_Turn()
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

      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);

      var controllerMap = new TeamRoleMap<ICharacterDescisionMaker>();
      controllerMap.SetItem(Team.Red, Role.Planter, CreateTestController());
      controllerMap.SetItem(Team.Red, Role.Harvester, CreateTestController());
      controllerMap.SetItem(Team.Red, Role.Worm, CreateTestController());
      controllerMap.SetItem(Team.Blue, Role.Planter, CreateTestController());
      controllerMap.SetItem(Team.Blue, Role.Harvester, CreateTestController());
      controllerMap.SetItem(Team.Blue, Role.Worm, CreateTestController());

      gameLogic.BindDecisionMakers(controllerMap);
      var mockRecorder = new MockRecorder();

      var task = gameLogic.PlayGame(mockRecorder);
      yield return new WaitUntil(() => task.IsCompleted);
      Assert.AreEqual(gameConfig.gameLength, mockRecorder.turnNumber);
    }

    private class MockRecorder : IReplayRecorder
    {
      public int turnNumber = 0;
      public void LogEndGame(ServerGameState serverGameState)
      {
        turnNumber = serverGameState.turn;
      }

      public void LogGameStart(int gameLogicVersion, GameConfig gameRule, MapInfo mapInfo)
      {
        turnNumber = 0;
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

    WebServiceDecisionMaker CreateTestController(string startPath = "/start", string turnPath = "/turn", string namePath = "/name")
    {
      var controller = new WebServiceDecisionMaker(
        "http://localhost:8686",
        startPath,
        turnPath,
        namePath
      );

      controller.Character = new Character(0, 0, Team.Red, Role.Harvester);

      return controller;
    }
  }
}
