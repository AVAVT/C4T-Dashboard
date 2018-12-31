using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class BotStatusContainer : SerializedMonoBehaviour
{
  public Dictionary<Team, RectTransform> teamContainers;
  public GameObject botStatusIndicatorPrefab;

  TeamRoleMap<BotStatusIndicator> indicators;

  public void CreateIndicatorForBots(TeamRoleMap<ICharacterDescisionMaker> bots)
  {
    indicators = new TeamRoleMap<BotStatusIndicator>();

    foreach (var team in teamContainers.Keys)
    {
      foreach (var kvp in bots.GetItemsBy(team))
      {
        var indicator = Instantiate(botStatusIndicatorPrefab, teamContainers[team]).GetComponent<BotStatusIndicator>();
        indicator.BindBot(kvp.Value as WebServiceDecisionMaker, team, kvp.Key);
        indicators.SetItem(team, kvp.Key, indicator);
      }
    }
  }

  public async Task<TeamRoleMap<string>> CheckBotsConnectivity()
  {
    TeamRoleMap<string> names = new TeamRoleMap<string>();
    bool allReady = true;
    var dictionary = indicators.ToDictionary();
    foreach (var team in dictionary.Keys)
    {
      foreach (var role in dictionary[team].Keys)
      {
        var indicator = dictionary[team][role];
        var name = await indicator.CheckStatus();
        if (name == null) allReady = false;
        else names.SetItem(team, role, name);
      }
    }

    if (allReady) return names;
    else throw new UnableToConnectException("Unable to connect");
  }
}
