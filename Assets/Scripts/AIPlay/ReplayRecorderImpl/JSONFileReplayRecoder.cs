using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class JSONFileReplayRecoder : IReplayRecorder
{
  public PlayRecordData RecordData { get; private set; }
  string exportPath;
  public string fileName { get; private set; }
  public Action<int, GameConfig, MapInfo> OnStart;
  public Action<ServerGameState, List<TurnAction>> OnTurn;
  public Action<ServerGameState> OnEnd;
  public JSONFileReplayRecoder(string exportPath, TeamRoleMap<string> playerNames)
  {
    this.exportPath = exportPath;
    RecordData = new PlayRecordData();
    RecordData.playerNames = playerNames.ToDictionary();
  }

  public void LogGameStart(int gameLogicVersion, GameConfig gameRule, MapInfo mapInfo)
  {
    RecordData.gameLogicVersion = gameLogicVersion;
    RecordData.gameRule = gameRule;
    RecordData.mapInfo = SaveDataHelper.SaveMapInfoFrom(mapInfo);
    RecordData.turnActions = new List<List<TurnAction>>();
    OnStart?.Invoke(gameLogicVersion, gameRule, mapInfo);
  }

  public void LogTurn(ServerGameState serverGameState, List<TurnAction> actions)
  {
    var turnIndex = serverGameState.turn - 1;
    if (turnIndex < RecordData.turnActions.Count) RecordData.turnActions[turnIndex] = actions;
    else RecordData.turnActions.Add(actions);

    OnTurn?.Invoke(serverGameState, actions);
  }

  public void LogEndGame(ServerGameState serverGameState)
  {
    RecordData.ISOTime = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK");
    var jsonString = JsonConvert.SerializeObject(RecordData);

    fileName = $"{DateTime.Now.ToString("ddMMyyyy_HHmmss")}.json";
    System.IO.FileInfo file = new System.IO.FileInfo(Path.Combine(exportPath, fileName));
    file.Directory.Create();
    System.IO.File.WriteAllText(file.FullName, jsonString);

    OnEnd?.Invoke(serverGameState);
  }
}