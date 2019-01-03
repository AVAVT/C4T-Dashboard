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
  public GameObject actionObject;
  public Transform actionArrow;

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
    if (action == null)
    {
      badStatusAvatar.gameObject.SetActive(false);
      badStatusPill.SetActive(false);
      actionObject.SetActive(false);
    }
    else
    {
      badStatusAvatar.gameObject.SetActive(action.crashed || action.timedOut);
      badStatusPill.SetActive(action.crashed || action.timedOut);
      badStatusPillText.text = action.crashed ? "CRASHED" : "TIMEOUT";

      actionObject.SetActive(!action.crashed && !action.timedOut);
      var direction = action.direction.ToDirectionVector();
      direction.Y = -direction.Y;
      var degree = Vector2.SignedAngle(Vector2.down, new Vector2(direction.X, direction.Y));
      actionArrow.transform.localRotation = Quaternion.Euler(0, 0, degree);
    }
  }
}
