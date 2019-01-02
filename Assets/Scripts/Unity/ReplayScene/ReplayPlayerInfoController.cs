using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplayPlayerInfoController : SerializedMonoBehaviour
{
  public Image avatar;
  public TMP_Text playerNameText;
  public GameObject badStatusPill;
  public TMP_Text badStatusPillText;
  public Image badStatusAvatar;
  public Dictionary<Team, Dictionary<Role, Sprite>> avatars;
  public Dictionary<Team, Dictionary<Role, Sprite>> badStatusAvatars;

  public void Initialize(Team team, Role role, string playerName)
  {
    playerNameText.text = playerName;
    badStatusAvatar.sprite = badStatusAvatars[team][role];
    avatar.sprite = avatars[team][role];
    badStatusAvatar.gameObject.SetActive(false);
    badStatusPill.SetActive(false);
  }

  public void UpdateState(TurnAction action)
  {
    if (action.crashed || action.timedOut)
    {
      badStatusAvatar.gameObject.SetActive(true);
      badStatusPill.SetActive(true);
      badStatusPillText.text = action.crashed ? "CRASHED" : "TIMEOUT";
    }
  }
}
