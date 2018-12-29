using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace AIPlayTests
{
  public class JSONFileReplayRecorderTests
  {
    [UnityTest]
    public IEnumerator Game_Recorded_To_Correct_File()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var gameLogic = CreateTestGameLogic(texture);

      var exportPath = System.IO.Path.Combine(Application.dataPath, "Tests", "GeneratedTestData");
      var testRecorder = new JSONFileReplayRecoder(
        exportPath,
        new TeamRoleMap<string>()
      );

      var task = gameLogic.PlayGame(testRecorder);
      yield return new WaitUntil(() => task.IsCompleted);
      FileAssert.Exists(new System.IO.FileInfo(Path.Combine(exportPath, testRecorder.fileName)));
    }

    IEnumerator LoadTextMapTexture(Action<Texture2D> callback)
    {
      var www = UnityWebRequestTexture.GetTexture("file://" + System.IO.Path.Combine(Application.streamingAssetsPath, "MapTextures/11x9.png"));
      yield return www.SendWebRequest();
      callback(((DownloadHandlerTexture)www.downloadHandler).texture);
    }

    GameLogic CreateTestGameLogic(Texture2D texture)
    {
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
      controllerMap.SetItem(Team.Red, Role.Planter, new MockAIDecisionMaker());
      controllerMap.SetItem(Team.Red, Role.Harvester, new MockAIDecisionMaker());
      controllerMap.SetItem(Team.Red, Role.Worm, new MockAIDecisionMaker());
      controllerMap.SetItem(Team.Blue, Role.Planter, new MockAIDecisionMaker());
      controllerMap.SetItem(Team.Blue, Role.Harvester, new MockAIDecisionMaker());
      controllerMap.SetItem(Team.Blue, Role.Worm, new MockAIDecisionMaker());
      gameLogic.BindDecisionMakers(controllerMap);

      return gameLogic;
    }

    private class MockAIDecisionMaker : ICharacterDescisionMaker
    {
      private Character character;
      public Character Character { get => character; set => character = value; }
      public bool IsTimedOut => false;
      public bool IsCrashed => false;
      public async Task DoStart(GameState gameState, GameConfig gameRule) { }
      public async Task<string> DoTurn(GameState gameState, GameConfig gameRule)
      {
        return Directions.DOWN;
      }
    }
  }
}