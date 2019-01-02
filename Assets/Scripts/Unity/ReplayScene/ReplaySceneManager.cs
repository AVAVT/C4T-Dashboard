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
  public ReplayScoreDisplayController scoreDisplayController;
  public GameObject loadingCanvas;
  public RectTransform redTeamInfoPanel;
  public RectTransform blueTeamInfoPanel;
  public GameObject redTeamInfoPrefab;
  public GameObject blueTeamInfoPrefab;

  private List<ServerGameState> serverGameStates = new List<ServerGameState>();
  private TeamRoleMap<ReplayPlayerInfoController> playerInfoControllers = new TeamRoleMap<ReplayPlayerInfoController>();

  private int currentTurn = -1;
  public int CurrentTurn
  {
    get { return currentTurn; }
    set
    {
      if (value >= 0 && value < serverGameStates.Count && currentTurn != value)
      {
        currentTurn = value;
        VisualizeState();
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
    CurrentTurn++;
  }
  public void PrevTurn()
  {
    CurrentTurn--;
  }
  public void GoToTurn(int turn)
  {
    CurrentTurn = turn;
  }

  void VisualizeState()
  {
    gameMapController.VisualizeState(serverGameStates[CurrentTurn]);
    controlPanelController.UpdateTurn(CurrentTurn);
    scoreDisplayController.VisualizeState(serverGameStates[CurrentTurn]);
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
    foreach (var character in serverGameStates[0].characters)
    {
      var team = character.team;
      var role = character.role;
      var playerInfo = (team == Team.Red ? Instantiate(redTeamInfoPrefab, redTeamInfoPanel) : Instantiate(blueTeamInfoPrefab, blueTeamInfoPanel)).GetComponent<ReplayPlayerInfoController>();
      playerInfoControllers.SetItem(team, role, playerInfo);
      playerInfo.Initialize(team, role, recordData.playerNames[team][role]);
    }
    gameMapController.InitializeMap(serverGameStates[0]);
    gameMapController.VisualizeState(serverGameStates[0]);
    controlPanelController.Initialize(recordData.gameRule.gameLength);
    CurrentTurn = 0;
    loadingCanvas.SetActive(false);
  }
}
