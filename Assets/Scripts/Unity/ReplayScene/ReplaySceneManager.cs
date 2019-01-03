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

  public float baseAnimationDuration = 0.5f;
  public List<float> speedTiers;
  private int currentSpeedTier;

  private List<ServerGameState> serverGameStates = new List<ServerGameState>();
  private List<List<TurnAction>> turnActions;
  private TeamRoleMap<ReplayPlayerInfoController> playerInfoControllers = new TeamRoleMap<ReplayPlayerInfoController>();

  private int currentTurn = -1;
  public int CurrentTurn
  {
    get { return currentTurn; }
    private set
    {
      if (value >= 0 && value < serverGameStates.Count && currentTurn != value)
      {
        currentTurn = value;
        VisualizeState();
      }
    }
  }

  private bool shouldAnimate = true;
  private bool autoPlay = true;

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
    currentSpeedTier = speedTiers.Count / 2;
    controlPanelController.UpdateSpeed(speedTiers[currentSpeedTier]);

    StartCoroutine(Initialize());
  }
  public void PlayOrPause()
  {
    autoPlay = !autoPlay;
    controlPanelController.UpdateAutoplayState(autoPlay);
    if (autoPlay) NextTurn();
  }

  public void NextTurn()
  {
    shouldAnimate = true;
    CurrentTurn++;
  }
  public void PrevTurn()
  {
    autoPlay = false;
    shouldAnimate = false;
    CurrentTurn--;
  }
  public void GoToTurn(int turn)
  {
    if (CurrentTurn != turn)
    {
      autoPlay = false;
      shouldAnimate = false;
      CurrentTurn = turn;
    }
  }

  public void IncreaseSpeed()
  {
    currentSpeedTier = Mathf.Min(currentSpeedTier + 1, speedTiers.Count - 1);
    controlPanelController.UpdateSpeed(speedTiers[currentSpeedTier]);
  }
  public void DecreaseSpeed()
  {
    currentSpeedTier = Mathf.Max(0, currentSpeedTier - 1);
    controlPanelController.UpdateSpeed(speedTiers[currentSpeedTier]);
  }

  void VisualizeState()
  {
    var currentGameState = serverGameStates[CurrentTurn];
    controlPanelController.UpdateAutoplayState(autoPlay);
    if (shouldAnimate)
    {
      controlPanelController.UpdateAnimatingState(true);
      gameMapController.AnimateState(
        currentGameState,
        turnActions[CurrentTurn - 1],
        baseAnimationDuration / speedTiers[currentSpeedTier],
        () =>
        {
          controlPanelController.UpdateAnimatingState(false);
          if (autoPlay) NextTurn();
        }
      );
    }
    else
    {
      controlPanelController.UpdateAnimatingState(false);
      gameMapController.VisualizeState(currentGameState);
    }
    controlPanelController.UpdateTurn(CurrentTurn);
    scoreDisplayController.VisualizeState(currentGameState);

    var playerInfoControllersDict = playerInfoControllers.ToDictionary();
    foreach (var team in playerInfoControllersDict.Keys)
    {
      foreach (var kvp in playerInfoControllersDict[team])
        kvp.Value.UpdateState(
          CurrentTurn > 0
          ? turnActions[CurrentTurn - 1].Find(a => a.team == team && a.role == kvp.Key)
          : null
        );
    }
  }

  IEnumerator Initialize()
  {
    yield return null;
    var gameLogic = GameLogic.GameLogicForNewGame(recordData.gameRule, SaveDataHelper.MapInfoFrom(recordData.mapInfo));
    serverGameStates.Add(gameLogic.GetGameStateSnapShot());
    turnActions = recordData.turnActions;
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
    GoToTurn(0);
    loadingCanvas.SetActive(false);
    PlayOrPause();
  }
}
