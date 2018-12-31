using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;
using System.Threading.Tasks;

public class BotStatusIndicator : SerializedMonoBehaviour
{
  public Image statusIcon;
  public Image roleIcon;
  public TMP_Text nameText;
  public TMP_Text roleText;
  public Sprite statusErrSprite;
  public Sprite statusOkSprite;
  public Dictionary<Team, Dictionary<Role, Sprite>> roleSprites;
  public WebServiceDecisionMaker decisionMaker { get; private set; }
  private Team team;
  private Role role;
  public void BindBot(WebServiceDecisionMaker decisionMaker, Team team, Role role)
  {
    this.decisionMaker = decisionMaker;
    this.team = team;
    this.role = role;
    statusIcon.sprite = statusErrSprite;
    roleIcon.sprite = roleSprites[team][role];
    if ((int)role % 2 > 0) GetComponent<Image>().enabled = true;
  }

  public async Task<string> CheckStatus()
  {
    nameText.text = "Checking connection...";
    await decisionMaker.Handshake((int)team, (int)role);

    if (decisionMaker.IsReady)
    {
      statusIcon.sprite = statusOkSprite;
      nameText.text = decisionMaker.PlayerName;
      return decisionMaker.PlayerName;
    }
    else
    {
      statusIcon.sprite = statusErrSprite;
      nameText.text = decisionMaker.PlayerName;
      return null;
    }
  }
}
