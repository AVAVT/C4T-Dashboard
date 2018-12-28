using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTextureHelper
{
  private static Dictionary<Color, TileType> mapColorCode;
  public static Dictionary<Color, TileType> MapColorCode
  {
    get
    {
      if (mapColorCode == null)
      {
        mapColorCode = new Dictionary<Color, TileType>()
        {
          {Color.white, TileType.EMPTY},
          {Color.black, TileType.IMPASSABLE},
          {Color.red, TileType.RED_BOX},
          {Color.blue, TileType.BLUE_BOX},
          {Color.magenta, TileType.WILDBERRY},
          {new Color(1,1,0), TileType.RED_ROCK},
          {new Color(0,1,1), TileType.BLUE_ROCK}
        };
      }
      return mapColorCode;
    }
  }
  public static MapInfo MapInfoFromTexture2D(Texture2D mapTileData)
  {
    var mapInfo = new MapInfo();

    for (int x = 0; x < mapTileData.width; x++)
    {
      mapInfo.tiles.Add(new List<TileType>());

      for (int y = mapTileData.height - 1; y >= 0; y--)
      {
        TileType tileType = TypeFromColor(mapTileData.GetPixel(x, y));
        mapInfo.tiles[x].Add(tileType);

        if (tileType == TileType.RED_BOX)
        {
          mapInfo.startingPositions.SetItem(Team.Red, Role.Planter, new System.Numerics.Vector2(x, mapTileData.height - y - 1));
          mapInfo.startingPositions.SetItem(Team.Red, Role.Harvester, new System.Numerics.Vector2(x, mapTileData.height - y - 1));
        }
        else if (tileType == TileType.BLUE_BOX)
        {
          mapInfo.startingPositions.SetItem(Team.Blue, Role.Planter, new System.Numerics.Vector2(x, mapTileData.height - y - 1));
          mapInfo.startingPositions.SetItem(Team.Blue, Role.Harvester, new System.Numerics.Vector2(x, mapTileData.height - y - 1));
        }
        else if (tileType == TileType.RED_ROCK)
        {
          mapInfo.startingPositions.SetItem(Team.Red, Role.Worm, new System.Numerics.Vector2(x, mapTileData.height - y - 1));
        }
        else if (tileType == TileType.BLUE_ROCK)
        {
          mapInfo.startingPositions.SetItem(Team.Blue, Role.Worm, new System.Numerics.Vector2(x, mapTileData.height - y - 1));
        }
      }
    }
    return mapInfo;
  }

  private static TileType TypeFromColor(Color color)
  {
    TileType result;
    if (MapColorCode.TryGetValue(color, out result)) return result;
    else
    {
      Debug.LogWarning($"Invalid color detected in map texture: {color}");
      return TileType.EMPTY;
    }
  }
}
