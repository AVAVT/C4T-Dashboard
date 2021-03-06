﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapVisualizer : MonoBehaviour
{
  public Vector2 bounds;
  public float paddingWidth;
  public float paddingHeight;
  public Sprite randomImage;
  public Sprite mapBorderImage;
  public GameObject lightTilePrefab;
  public GameObject darkTilePrefab;
  public GameObject redBoxPrefab;
  public GameObject blueBoxPrefab;
  public GameObject redRockPrefab;
  public GameObject blueRockPrefab;
  public GameObject wildBerryPrefab;
  public GameObject waterPrefab;

  private RectTransform rectTransform;
  private GridLayoutGroup gridLayoutGroup;
  private Image image;

  private void Start()
  {
    image = GetComponent<Image>();
    rectTransform = transform as RectTransform;
    gridLayoutGroup = GetComponent<GridLayoutGroup>();
    GetComponent<Image>().enabled = false;
  }

  public void VisualizeMap(MapInfo mapInfo)
  {
    rectTransform.DestroyAllChildren();
    image.enabled = true;

    if (mapInfo == null) VisualizeIconRandom();
    else VisualizeMapInfo(mapInfo);
  }

  void VisualizeIconRandom()
  {
    image.sprite = randomImage;
    image.type = Image.Type.Simple;
    image.SetNativeSize();
  }

  void VisualizeMapInfo(MapInfo mapInfo)
  {
    image.sprite = mapBorderImage;
    image.type = Image.Type.Sliced;

    var mapWidth = mapInfo.tiles.Count;
    var mapHeight = mapInfo.tiles[0].Count;
    var tileSize = Mathf.Min(bounds.x / mapWidth, bounds.y / mapHeight);

    rectTransform.sizeDelta = new Vector2(
      mapWidth * tileSize + paddingWidth,
      mapHeight * tileSize + paddingHeight
    );
    gridLayoutGroup.cellSize = Vector2.one * tileSize;

    for (int y = 0; y < mapHeight; y++)
    {
      for (int x = 0; x < mapWidth; x++)
      {
        var prefab = x % 2 == y % 2 ? lightTilePrefab : darkTilePrefab;
        var newTileTransform = Instantiate(prefab, transform).GetComponent<RectTransform>();
        var tileType = mapInfo.tiles[x][y];

        if (tileType == TileType.EMPTY) continue;

        GameObject itemPrefab = null;
        if (tileType == TileType.RED_BOX) itemPrefab = redBoxPrefab;
        else if (tileType == TileType.BLUE_BOX) itemPrefab = blueBoxPrefab;
        else if (tileType == TileType.RED_ROCK) itemPrefab = redRockPrefab;
        else if (tileType == TileType.BLUE_ROCK) itemPrefab = blueRockPrefab;
        else if (tileType == TileType.WILDBERRY) itemPrefab = wildBerryPrefab;
        else if (tileType == TileType.IMPASSABLE) itemPrefab = waterPrefab;
        if (prefab == null) continue;

        var itemTransform = Instantiate(itemPrefab, newTileTransform).GetComponent<RectTransform>();
        itemTransform.offsetMax = Vector2.zero;
        itemTransform.offsetMin = Vector2.zero;
      }
    }
  }
}
