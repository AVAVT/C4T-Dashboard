using System.Collections;
using System.Collections.Generic;
using System.IO;
using Crosstales.FB;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneManager : MonoBehaviour
{
  public NotificationController notificationController;
  public void OnExitClick()
  {
    Application.Quit();
  }
  public void OnNewGameClick()
  {
    SceneManager.LoadScene("AIPlayScene");
  }
  public void OnReplayClick()
  {
    var directory = PlayerPrefs.GetString(AIPlaySceneManager.LOG_FILE_LOCATION_KEY, "");
    string path = FileBrowser.OpenSingleFile("Choose log file", directory, "json");

    if (!string.IsNullOrEmpty(path) && File.Exists(path))
    {
      using (StreamReader reader = new StreamReader(path))
      {
        try
        {
          var jsonString = reader.ReadToEnd();
          PlayRecordData data = JsonConvert.DeserializeObject<PlayRecordData>(jsonString);
          ReplaySceneManager.recordData = data;
          SceneManager.LoadScene("ReplayScene");
        }
        catch (System.Exception e)
        {
          Debug.Log(e);
          notificationController.ShowNotification("That is not a valid game log file!", 2);
        }
      }
    }
    else notificationController.ShowNotification("File does not exists!", 2);
  }
}
