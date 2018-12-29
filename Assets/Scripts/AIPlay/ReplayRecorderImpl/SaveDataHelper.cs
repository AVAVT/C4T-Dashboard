public static class SaveDataHelper
{
  public static SaveGameState SaveGameStateFrom(ServerGameState serverGameState)
  {
    var result = new SaveGameState();
    result.turn = serverGameState.turn;
    result.redScore = serverGameState.redScore;
    result.blueScore = serverGameState.blueScore;
    result.map = serverGameState.map;
    result.characters = serverGameState.characters.ToDictionary();

    return result;
  }

  public static ServerGameState ServerGameStateFrom(SaveGameState saveGameState)
  {
    var result = new ServerGameState();
    result.turn = saveGameState.turn;
    result.redScore = saveGameState.redScore;
    result.blueScore = saveGameState.blueScore;
    result.map = saveGameState.map;
    result.characters = new TeamRoleMap<Character>(saveGameState.characters);

    return result;
  }

  public static MapInfo MapInfoFrom(SaveMapInfo saveMapInfo)
  {
    var result = new MapInfo();
    result.tiles = saveMapInfo.tiles;
    result.startingPositions = new TeamRoleMap<System.Numerics.Vector2>(saveMapInfo.startingPositions);

    return result;
  }

  public static SaveMapInfo SaveMapInfoFrom(MapInfo mapInfo)
  {
    var result = new SaveMapInfo();
    result.tiles = mapInfo.tiles;
    result.startingPositions = mapInfo.startingPositions.ToDictionary();

    return result;
  }
}