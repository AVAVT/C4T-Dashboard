using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class MapChooser : MonoBehaviour
{
  public string mapTexturePath = "MapTextures";
  public MapVisualizer mapVisualizer;
  public GameObject buttonNext;
  public GameObject buttonPrev;

  List<MapInfo> mapInfos = new List<MapInfo>();
  int mapInfoIndex = -1;

  private IEnumerator Start()
  {
    buttonNext.SetActive(true);
    buttonPrev.SetActive(true);

    var path = Path.Combine(Application.streamingAssetsPath, mapTexturePath);
    var info = new DirectoryInfo(path);
    var filesInfo = info.GetFiles();
    foreach (var file in filesInfo)
    {
      if (file.Extension != ".png") continue;

      yield return LoadTextMapTexture(
        Path.Combine(path, file.FullName),
        (texture) => mapInfos.Add(MapTextureHelper.MapInfoFromTexture2D(texture))
      );
    }

    if (mapInfos.Count > 0)
    {
      mapInfoIndex = 0;
      buttonNext.SetActive(true);
      buttonPrev.SetActive(true);
      VisualizeMap();
    }
  }

  public MapInfo GetChosenMapInfo()
  {
    if (mapInfoIndex >= 0) return mapInfos[mapInfoIndex];
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
    mapVisualizer.VisualizeMap(mapInfos[mapInfoIndex]);
  }

  IEnumerator LoadTextMapTexture(string path, Action<Texture2D> callback)
  {
    var www = UnityWebRequestTexture.GetTexture($"file://{path}");
    yield return www.SendWebRequest();
    callback(((DownloadHandlerTexture)www.downloadHandler).texture);
  }
}
