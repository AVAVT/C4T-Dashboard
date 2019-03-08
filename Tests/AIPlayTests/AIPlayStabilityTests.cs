using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace AIPlayTests
{
  public class AIPlayStabilityTests
  {
    static readonly string TEST_SERVER_URI = "http://localhost:8686";
    System.Diagnostics.Process process;

    [OneTimeSetUp]
    public void SetupMockServer()
    {
      if (TestUtility.IsWindows())
      {
        process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "node.exe";
        Debug.Log(Application.dataPath);
        process.StartInfo.Arguments = $"\"{Application.dataPath}/Tests/MockAIServer-JS/index.js\"";
        process.Start();
      }
      else
      {
        Debug.LogWarning("!!WARNING!! OS is not Windows. Test server will need to be started manually. It is localed at:");
        Debug.LogWarning($"\"{Application.dataPath}/Tests/MockAIServer-JS/index.js\"");
      }
    }

    [OneTimeTearDown]
    public void StopMockServer()
    {
      if (TestUtility.IsWindows())
      {
        process.Kill();
      }
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
      var controller = new WebServiceDecisionMaker(TEST_SERVER_URI);
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
    [Timeout(100000000)]
    public IEnumerator Work_Until_Last_Turn()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);
      var gameConfig = GameConfig.DefaultGameRule();

      var gameLogic = GameLogic.GameLogicForNewGame(gameConfig, mapInfo);

      var controllerMap = new TeamRoleMap<ICharacterDescisionMaker>();
      controllerMap.SetItem(Team.Red, Role.Planter, CreateTestController());
      controllerMap.SetItem(Team.Red, Role.Harvester, CreateTestController());
      controllerMap.SetItem(Team.Red, Role.Worm, CreateTestController());
      controllerMap.SetItem(Team.Blue, Role.Planter, CreateTestController());
      controllerMap.SetItem(Team.Blue, Role.Harvester, CreateTestController());
      controllerMap.SetItem(Team.Blue, Role.Worm, CreateTestController());

      gameLogic.BindDecisionMakers(controllerMap);
      var exportPath = Path.Combine(Application.dataPath, "Tests", "GeneratedTestData");
      var playerNames = new TeamRoleMap<string>();
      playerNames.SetItem(Team.Red, Role.Planter, "RedPlanter");
      playerNames.SetItem(Team.Red, Role.Harvester, "RedHarvester");
      playerNames.SetItem(Team.Red, Role.Worm, "RedWorm");
      playerNames.SetItem(Team.Blue, Role.Planter, "BluePlanter");
      playerNames.SetItem(Team.Blue, Role.Harvester, "BlueHarvester");
      playerNames.SetItem(Team.Blue, Role.Worm, "BlueWorm");

      var recorder = new JSONFileReplayRecoder(exportPath, playerNames);

      var tokenSource = new CancellationTokenSource();
      var ct = tokenSource.Token;
      var task = gameLogic.PlayGame(ct, recorder);
      yield return new WaitUntil(() => task.IsCompleted);
      if (task.IsCanceled || task.IsFaulted) Debug.LogError(task.Exception);
      Assert.DoesNotThrow(() =>
      {
        var path = Path.Combine(exportPath, recorder.fileName);
        using (StreamReader reader = new StreamReader(path))
        {
          var jsonString = reader.ReadToEnd();
          PlayRecordData data = JsonConvert.DeserializeObject<PlayRecordData>(jsonString);
          Assert.AreEqual(data.turnActions.Count, gameConfig.gameLength);
        }
      });
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
        TEST_SERVER_URI,
        startPath,
        turnPath,
        namePath
      );

      return controller;
    }
  }
}
