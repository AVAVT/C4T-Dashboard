using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class ReplayResultDisplayController : MonoBehaviour
{
  public Color redTeamColor;
  public Color blueTeamColor;
  public TMP_Text messageText;

  Tween colorTween;

  public void HideDisplay()
  {
    gameObject.SetActive(false);
    if (colorTween != null && !colorTween.IsComplete())
    {
      colorTween.Kill();
      colorTween = null;
    }
  }
  public void ShowDisplay(bool isRedTeamWin, float duration)
  {
    gameObject.SetActive(true);
    messageText.text = isRedTeamWin ? "Red Team Win!" : "Blue Team Win!";
    messageText.color = Color.clear;
    colorTween = messageText.DOColor(isRedTeamWin ? redTeamColor : blueTeamColor, duration).SetDelay(duration);
  }
}
