using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MapTextureHelperTests
{
  public class MapTextureHelperTests
  {
    [UnityTest]
    public IEnumerator MapInfo_Initialized_Not_NULL()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);

      Assert.IsNotNull(mapInfo);
    }

    [UnityTest]
    public IEnumerator MapInfo_Initialized_Correct_Size()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);

      Assert.AreEqual(11, mapInfo.tiles.Count);
      Assert.AreEqual(9, mapInfo.tiles[0].Count);
    }

    [UnityTest]
    public IEnumerator MapInfo_Initialized_Correct_Character_Position()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);

      Assert.AreEqual(new System.Numerics.Vector2(0, 0), mapInfo.startingPositions.GetItem(Team.Red, Role.Planter));
      Assert.AreEqual(new System.Numerics.Vector2(0, 0), mapInfo.startingPositions.GetItem(Team.Red, Role.Harvester));
      Assert.AreEqual(new System.Numerics.Vector2(0, 8), mapInfo.startingPositions.GetItem(Team.Red, Role.Worm));
      Assert.AreEqual(new System.Numerics.Vector2(10, 8), mapInfo.startingPositions.GetItem(Team.Blue, Role.Planter));
      Assert.AreEqual(new System.Numerics.Vector2(10, 8), mapInfo.startingPositions.GetItem(Team.Blue, Role.Harvester));
      Assert.AreEqual(new System.Numerics.Vector2(10, 0), mapInfo.startingPositions.GetItem(Team.Blue, Role.Worm));
    }

    [UnityTest]
    public IEnumerator MapInfo_Initialized_Correct_Wildberry_Position()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);


      Assert.AreEqual(TileType.WILDBERRY, mapInfo.tiles[2][1]);
      Assert.AreEqual(TileType.WILDBERRY, mapInfo.tiles[8][1]);
      Assert.AreEqual(TileType.WILDBERRY, mapInfo.tiles[5][4]);
      Assert.AreEqual(TileType.WILDBERRY, mapInfo.tiles[2][7]);
      Assert.AreEqual(TileType.WILDBERRY, mapInfo.tiles[8][7]);
    }

    [UnityTest]
    public IEnumerator MapInfo_Initialized_Correct_Water_Position()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);


      Assert.AreEqual(TileType.IMPASSABLE, mapInfo.tiles[5][1]);
      Assert.AreEqual(TileType.IMPASSABLE, mapInfo.tiles[2][4]);
      Assert.AreEqual(TileType.IMPASSABLE, mapInfo.tiles[8][4]);
      Assert.AreEqual(TileType.IMPASSABLE, mapInfo.tiles[5][7]);
    }

    [UnityTest]
    public IEnumerator MapInfo_Initialized_Correct_RedBox_Position()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);


      Assert.AreEqual(TileType.RED_BOX, mapInfo.tiles[0][0]);
    }

    [UnityTest]
    public IEnumerator MapInfo_Initialized_Correct_BlueBox_Position()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);


      Assert.AreEqual(TileType.BLUE_BOX, mapInfo.tiles[10][8]);
    }

    [UnityTest]
    public IEnumerator MapInfo_Initialized_Correct_RedRock_Position()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);


      Assert.AreEqual(TileType.RED_ROCK, mapInfo.tiles[0][8]);
    }

    [UnityTest]
    public IEnumerator MapInfo_Initialized_Correct_BlueRock_Position()
    {
      Texture2D texture = null;
      yield return LoadTextMapTexture(result => texture = result);
      var mapInfo = MapTextureHelper.MapInfoFromTexture2D(texture);


      Assert.AreEqual(TileType.BLUE_ROCK, mapInfo.tiles[10][0]);
    }

    IEnumerator LoadTextMapTexture(Action<Texture2D> callback)
    {
      var www = new WWW("file://" + System.IO.Path.Combine(Application.streamingAssetsPath, "MapTextures/11x9.png"));
      yield return www;
      callback(www.texture);
    }
  }
}
