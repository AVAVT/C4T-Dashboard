using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Crosstales.FB;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AIPlaySceneManager : MonoBehaviour
{
  public static readonly string LOG_FILE_LOCATION_KEY = "LOG_FILE_LOCATION_KEY";
  public string host;
  public string handshakePath;
  public string startPath;
  public string turnPath;

  public MapChooser mapChooser;
  public BotStatusContainer botStatusContainer;
  public Button connectivityButton;
  public Button playButton;
  public GameProgressCanvas gameProgressCanvas;
  public NotificationController notificationController;
  public TMP_Text logFileLocationText;

  private TeamRoleMap<ICharacterDescisionMaker> decisionMakers = new TeamRoleMap<ICharacterDescisionMaker>();
  private Dictionary<WebServiceDecisionMaker, BotStatusIndicator> indicators;

  private GameConfig gameRule;
  private CancellationTokenSource currentGameCancellationToken;
  private void Start()
  {
    gameProgressCanvas.OnCancel = CancelGame;
    logFileLocationText.text = PlayerPrefs.GetString(LOG_FILE_LOCATION_KEY, "Log file location not set");

    gameRule = GameConfig.DefaultGameRule();
    foreach (var team in gameRule.availableTeams)
    {
      foreach (var role in gameRule.availableRoles)
      {
        decisionMakers.SetItem(team, role, new WebServiceDecisionMaker(host, startPath, turnPath, handshakePath));
      }
    }

    botStatusContainer.CreateIndicatorForBots(decisionMakers);
    CheckBotsConnectivity();
  }
  public void OnExitClick()
  {
    SceneManager.LoadScene("MenuScene");
  }
  public void OnPlayButtonClick()
  {
    if (!PlayerPrefs.HasKey(LOG_FILE_LOCATION_KEY))
    {
      notificationController.ShowNotification(
        "Log file location not set.\nPlease choose where to save game log file.",
        () => ChooseLogFileLocation((path) => PlayGame())
      );
    }
    else PlayGame();
  }

  async void PlayGame()
  {
    try
    {
      var gameLogic = GameLogic.GameLogicForNewGame(gameRule, mapChooser.GetChosenMapInfo());
      gameLogic.BindDecisionMakers(decisionMakers as TeamRoleMap<ICharacterDescisionMaker>);

      if (!PlayerPrefs.HasKey(LOG_FILE_LOCATION_KEY)) return;

      var exportPath = PlayerPrefs.GetString(LOG_FILE_LOCATION_KEY);
      var recorder = new JSONFileReplayRecoder(exportPath, await GetBotNames());
      recorder.OnStart = (logicVersion, gameConfig, mapInfo) =>
      {
        gameProgressCanvas.OnStart(gameConfig.gameLength);
      };
      recorder.OnTurn = (gameState, actions) =>
      {
        gameProgressCanvas.OnTurn(gameState.turn);
      };
      recorder.OnEnd = (gameState) =>
      {
        gameProgressCanvas.OnEnd();
      };
      currentGameCancellationToken = new CancellationTokenSource();
      var ct = currentGameCancellationToken.Token;
      await gameLogic.PlayGame(ct, recorder);
      currentGameCancellationToken.Dispose();
      notificationController.ShowNotification(
        $"Game finished. Log file saved with name\n{recorder.fileName}\n"
      );
    }
    catch (NotInitializedException e)
    {
      Debug.Log(e);
    }
  }

  public void CancelGame()
  {
    currentGameCancellationToken?.Cancel();
  }
  public void OnConnectivityButtonClick()
  {
    CheckBotsConnectivity();
  }
  async void CheckBotsConnectivity()
  {
    connectivityButton.interactable = false;
    playButton.interactable = false;

    try
    {
      await GetBotNames();
      connectivityButton.interactable = true;
      playButton.interactable = true;
    }
    catch (UnableToConnectException e)
    {
      Debug.Log(e);
      connectivityButton.interactable = true;
    }
  }

  async Task<TeamRoleMap<string>> GetBotNames()
  {
    try
    {
      var result = await botStatusContainer.CheckBotsConnectivity();
      return result;
    }
    catch (UnableToConnectException e)
    {
      throw e;
    }
  }

  public void OnLogFileLocationButtonClick()
  {
    ChooseLogFileLocation();
  }

  public void ChooseLogFileLocation(Action<string> OnSuccess = null, Action OnCancelled = null)
  {
    string path = FileBrowser.OpenSingleFolder("Choose where to save log file");

    if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) OnCancelled?.Invoke();
    else
    {
      PlayerPrefs.SetString(LOG_FILE_LOCATION_KEY, path);

      logFileLocationText.text = path;

      OnSuccess?.Invoke(path);
    }
  }
}
