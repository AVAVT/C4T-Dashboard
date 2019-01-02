using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class GameMapController : SerializedMonoBehaviour
{
  public Vector2 bounds;
  public float paddingWidth;
  public float paddingHeight;
  public GameObject tilePrefab;
  public GameObject groundObjectPrefab;
  public Color lightTileColor;
  public Color darkTileColor;
  public Sprite redBoxSprite;
  public Sprite blueBoxSprite;
  public Sprite redRockSprite;
  public Sprite blueRockSprite;
  public Sprite waterSprite;

  public List<Sprite> wildBerrySprites;
  public List<Sprite> tomatoSprites;
  public List<Sprite> pumpkinSprites;
  public Dictionary<Team, Dictionary<Role, Sprite>> roleSprites;

  public RectTransform boardRectTransform;
  public GridLayoutGroup boardGridLayoutGroup;

  private List<List<Image>> tiles;


  public void InitializeMap(ServerGameState gameState)
  {
    var mapWidth = gameState.map.Count;
    var mapHeight = gameState.map[0].Count;
    var tileSize = Mathf.Min(bounds.x / mapWidth, bounds.y / mapHeight);

    (transform as RectTransform).sizeDelta = new Vector2(
      mapWidth * tileSize + paddingWidth,
      mapHeight * tileSize + paddingHeight
    );
    boardGridLayoutGroup.cellSize = Vector2.one * tileSize;

    boardRectTransform.DestroyAllChildren();

    tiles = new List<List<Image>>();
    for (int x = 0; x < mapWidth; x++)
    {
      tiles.Add(new List<Image>());

      for (int y = 0; y < mapHeight; y++)
      {
        var newTileTransform = Instantiate(tilePrefab, transform).GetComponent<RectTransform>();
        newTileTransform.GetComponent<Image>().color = x % 2 == y % 2 ? lightTileColor : darkTileColor;

        var itemTransform = Instantiate(groundObjectPrefab, Vector3.zero, Quaternion.identity, newTileTransform).GetComponent<RectTransform>();
        itemTransform.offsetMax = Vector2.zero;
        itemTransform.offsetMin = Vector2.zero;
        var image = itemTransform.GetComponent<Image>();
        image.sprite = null;
        image.color = Color.clear;
        tiles[x].Add(image);

      }
    }
  }

  public void VisualizeState(ServerGameState gameState)
  {
    tiles = new List<List<Image>>();
    for (int x = 0; x < gameState.map.Count; x++)
    {
      tiles.Add(new List<Image>());

      for (int y = 0; y < gameState.map[x].Count; y++)
      {
        var image = tiles[x][y];
        var tileType = gameState.map[x][y].type;
        var growState = gameState.map[x][y].growState;

        if (tileType == TileType.EMPTY)
        {
          image.color = Color.clear;
          continue;
        }
        else image.color = Color.white;

        if (tileType == TileType.WILDBERRY) image.sprite = wildBerrySprites[growState];
        else if (tileType == TileType.TOMATO) image.sprite = tomatoSprites[growState];
        else if (tileType == TileType.PUMPKIN) image.sprite = pumpkinSprites[growState];
        else if (tileType == TileType.RED_BOX) image.sprite = redBoxSprite;
        else if (tileType == TileType.BLUE_BOX) image.sprite = blueBoxSprite;
        else if (tileType == TileType.RED_ROCK) image.sprite = redRockSprite;
        else if (tileType == TileType.BLUE_ROCK) image.sprite = blueRockSprite;
        else if (tileType == TileType.IMPASSABLE) image.sprite = waterSprite;

        image.SetNativeSize();
      }
    }
  }
}
