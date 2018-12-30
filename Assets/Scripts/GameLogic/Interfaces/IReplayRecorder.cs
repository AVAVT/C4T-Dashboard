using System.Collections.Generic;

public interface IReplayRecorder
{
  void LogGameStart(int gameLogicVersion, GameConfig gameRule, MapInfo mapInfo);
  void LogTurn(ServerGameState serverGameState, List<TurnAction> actions);
  void LogEndGame(ServerGameState serverGameState);
}