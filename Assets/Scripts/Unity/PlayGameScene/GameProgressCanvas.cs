using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class GameProgressCanvas : MonoBehaviour
{
  public TMP_Text progressText;
  public RectTransform progressBar;
  public RectTransform progressFill;
  public Action OnCancel;

  private int gameLength;
  public void OnStart(int gameLength)
  {
    this.gameLength = gameLength;
    progressText.text = $"GAME IN PROGRESS: 0/{gameLength}";
    progressFill.sizeDelta = Vector2.zero;
    gameObject.SetActive(true);
  }

  public void OnTurn(int turn)
  {
    progressText.text = $"GAME IN PROGRESS: {turn}/{gameLength}";
    progressFill.sizeDelta = progressBar.sizeDelta.WithX(progressBar.sizeDelta.x * turn / gameLength);
  }

  public void OnEnd()
  {
    gameObject.SetActive(false);
  }

  public void OnCancelClicked()
  {
    OnCancel?.Invoke();
  }
}
