using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationController : MonoBehaviour
{
  public TMP_Text notificationText;
  public Button okButton;

  private Action OkButtonCallback;

  public void ShowNotification(string text, Action OnClose = null)
  {
    gameObject.SetActive(true);
    notificationText.text = text;
    okButton.gameObject.SetActive(true);
    OkButtonCallback = OnClose;
  }
  public void ShowNotification(string text, float autoHideTime)
  {
    gameObject.SetActive(true);
    notificationText.text = text;
    okButton.gameObject.SetActive(false);
    StartCoroutine(HideAfter(autoHideTime));
  }

  public void OnCloseButtonClick()
  {
    gameObject.SetActive(false);
    OkButtonCallback?.Invoke();
  }

  IEnumerator HideAfter(float time)
  {
    yield return new WaitForSeconds(time);
    gameObject.SetActive(false);
  }
}
