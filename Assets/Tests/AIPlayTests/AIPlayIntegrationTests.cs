﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace AIPlayTests
{
  public class AIPlayIntegrationTests
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

      var task = gameLogic.PlayGame(recorder);
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
        "http://localhost:8686",
        startPath,
        turnPath,
        namePath
      );

      return controller;
    }
  }
}
