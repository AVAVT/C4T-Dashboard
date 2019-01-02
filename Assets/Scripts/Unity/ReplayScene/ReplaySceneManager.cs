using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReplaySceneManager : MonoBehaviour
{
  public static PlayRecordData recordData;

  public NotificationController notificationController;
  public ReplayMapController gameMapController;
  public ReplayControlPanelController controlPanelController;
  public GameObject loadingCanvas;

  private List<ServerGameState> serverGameStates = new List<ServerGameState>();

  private int currentTurn = 0;
  public int CurrentTurn
  {
    get { return currentTurn; }
    set
    {
      if (value >= 0 && value < serverGameStates.Count && currentTurn != value)
      {
        currentTurn = value;
        UpdateMapVisual();
        controlPanelController.UpdateTurn(value);
      }
    }
  }

  private void Start()
  {
    loadingCanvas.SetActive(true);
    if (recordData == null)
    {
      notificationController.ShowNotification(
        "No record data set. Please choose one in the menu.",
        () => SceneManager.LoadScene("MenuScene")
      );
      return;
    }

    StartCoroutine(Initialize());
  }
  public void NextTurn()
  {
    if (CurrentTurn < serverGameStates.Count - 1) CurrentTurn++;
  }
  public void PrevTurn()
  {
    if (CurrentTurn > 0) CurrentTurn--;
  }
  public void GoToTurn(int turn)
  {
    CurrentTurn = turn;
  }

  void UpdateMapVisual()
  {
    gameMapController.VisualizeState(serverGameStates[CurrentTurn]);
  }

  IEnumerator Initialize()
  {
    yield return null;
    var gameLogic = GameLogic.GameLogicForNewGame(recordData.gameRule, SaveDataHelper.MapInfoFrom(recordData.mapInfo));
    serverGameStates.Add(gameLogic.GetGameStateSnapShot());
    foreach (var actions in recordData.turnActions)
    {
      gameLogic.ExecuteTurn(actions);
      serverGameStates.Add(gameLogic.GetGameStateSnapShot());
    }
    gameMapController.InitializeMap(serverGameStates[0]);
    gameMapController.VisualizeState(serverGameStates[0]);
    controlPanelController.Initialize(recordData.gameRule.gameLength);
    loadingCanvas.SetActive(false);
  }
}
