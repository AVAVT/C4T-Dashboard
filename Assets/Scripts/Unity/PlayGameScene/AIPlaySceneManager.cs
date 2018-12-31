using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AIPlaySceneManager : MonoBehaviour
{
  public string host;
  public string handshakePath;
  public string startPath;
  public string turnPath;

  public MapChooser mapChooser;
  public BotStatusContainer botStatusContainer;
  public Button connectivityButton;
  public Button playButton;

  private TeamRoleMap<ICharacterDescisionMaker> decisionMakers = new TeamRoleMap<ICharacterDescisionMaker>();
  private Dictionary<WebServiceDecisionMaker, BotStatusIndicator> indicators;

  private GameConfig gameRule;
  private async void Start()
  {
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
  public void OnPlayButtonClick()
  {
    PlayGame();
  }

  async void PlayGame()
  {
    try
    {
      var gameLogic = GameLogic.GameLogicForNewGame(gameRule, mapChooser.GetChosenMapInfo());
      gameLogic.BindDecisionMakers(decisionMakers as TeamRoleMap<ICharacterDescisionMaker>);

      var exportPath = System.IO.Path.Combine(Application.dataPath, "Tests", "GeneratedTestData");
      var recorder = new JSONFileReplayRecoder(exportPath, await GetBotNames());
      await gameLogic.PlayGame(recorder);
    }
    catch (NotInitializedException e)
    {
      Debug.Log(e);
    }
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
}
