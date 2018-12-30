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

  List<MapInfo> mapInfos = new List<MapInfo>();
  int mapInfoIndex = 0;

  private IEnumerator Start()
  {
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

    if (mapInfos.Count > 0) VisualizeMap();
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
