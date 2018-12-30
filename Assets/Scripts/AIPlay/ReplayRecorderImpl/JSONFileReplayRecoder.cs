using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class JSONFileReplayRecoder : IReplayRecorder
{
  PlayRecordData recordData;
  string exportPath;
  public string fileName { get; private set; }
  public JSONFileReplayRecoder(string exportPath, TeamRoleMap<string> playerNames)
  {
    this.exportPath = exportPath;
    recordData = new PlayRecordData();
    recordData.playerNames = playerNames.ToDictionary();
  }

  public void LogGameStart(int gameLogicVersion, GameConfig gameRule, MapInfo mapInfo)
  {
    recordData.gameLogicVersion = gameLogicVersion;
    recordData.gameRule = gameRule;
    recordData.mapInfo = SaveDataHelper.SaveMapInfoFrom(mapInfo);
    recordData.turnActions = new List<List<TurnAction>>();
  }

  public void LogTurn(ServerGameState serverGameState, List<TurnAction> actions)
  {
    var turnIndex = serverGameState.turn - 1;
    if (turnIndex < recordData.turnActions.Count) recordData.turnActions[turnIndex] = actions;
    else recordData.turnActions.Add(actions);
  }

  public void LogEndGame(ServerGameState serverGameState)
  {
    recordData.ISOTime = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK");
    var jsonString = JsonConvert.SerializeObject(recordData);

    fileName = $"{DateTime.Now.ToString("ddMMyyyy_HHmmss")}.json";
    System.IO.FileInfo file = new System.IO.FileInfo(Path.Combine(exportPath, fileName));
    file.Directory.Create();
    System.IO.File.WriteAllText(file.FullName, jsonString);
  }
}