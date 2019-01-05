using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class MapChooser : MonoBehaviour
{
  public string mapTexturePath = "MapTextures";
  public MapVisualizer mapVisualizer;
  public TMP_Text mapNameText;
  public GameObject buttonNext;
  public GameObject buttonPrev;

  List<MapVisualData> mapInfos = new List<MapVisualData>();
  int mapInfoIndex = -1;

  private IEnumerator Start()
  {
    buttonNext.SetActive(true);
    buttonPrev.SetActive(true);

    var path = Path.Combine(Application.streamingAssetsPath, mapTexturePath);
    var info = new DirectoryInfo(path);
    var filesInfo = info.GetFiles();
    mapInfos.Add(MapVisualData.DataForRandom());

    foreach (var file in filesInfo)
    {
      if (file.Extension != ".png") continue;

      yield return LoadTextMapTexture(
        Path.Combine(path, file.FullName),
        (texture) => mapInfos.Add(
          new MapVisualData(MapTextureHelper.MapInfoFromTexture2D(texture), Path.GetFileNameWithoutExtension(file.Name))
        )
      );
    }

    if (mapInfos.Count > 1)
    {
      mapInfoIndex = 0;
      buttonNext.SetActive(true);
      buttonPrev.SetActive(true);
      VisualizeMap();
    }
  }

  public MapInfo GetChosenMapInfo()
  {
    if (mapInfoIndex == 0) return mapInfos[UnityEngine.Random.Range(1, mapInfos.Count)].mapInfo;
    else if (mapInfoIndex > 0) return mapInfos[mapInfoIndex].mapInfo;
    else throw new NotInitializedException("Map data not finished loading");
  }

  public void NextMap()
  {
    mapInfoIndex = mapInfoIndex < mapInfos.Count - 1 ? mapInfoIndex + 1 : 0;
    VisualizeMap();
  }

  public void PrevMap()
  {
    mapInfoIndex = mapInfoIndex > 0 ? mapInfoIndex - 1 : mapInfos.Count - 1;
    VisualizeMap();
  }

  void VisualizeMap()
  {
    mapNameText.text = mapInfos[mapInfoIndex].name;
    mapVisualizer.VisualizeMap(mapInfos[mapInfoIndex].mapInfo);
  }

  IEnumerator LoadTextMapTexture(string path, Action<Texture2D> callback)
  {
    var www = UnityWebRequestTexture.GetTexture($"file://{path}");
    yield return www.SendWebRequest();
    callback(((DownloadHandlerTexture)www.downloadHandler).texture);
  }
}

public class MapVisualData
{
  public string name;
  public MapInfo mapInfo;

  public static MapVisualData DataForRandom()
  {
    return new MapVisualData(null, "Random Map");
  }

  public MapVisualData(MapInfo mapInfo, string name)
  {
    this.mapInfo = mapInfo;
    this.name = name;
  }
}
