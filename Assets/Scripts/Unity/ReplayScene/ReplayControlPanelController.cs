using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplayControlPanelController : MonoBehaviour
{
  public ReplaySceneManager replaySceneManager;

  public Slider slider;
  public TMP_Text turnText;
  public TMP_Text gameLengthText;

  public void Initialize(int gameLength)
  {
    slider.maxValue = gameLength;
    gameLengthText.text = gameLength.ToString();
    turnText.text = "0";
  }

  public void UpdateTurn(int turn)
  {
    turnText.text = turn.ToString();
    slider.value = turn;
  }

  public void OnSliderMoved(float value)
  {
    replaySceneManager.CurrentTurn = (int)value;
  }
  public void NextTurn()
  {
    replaySceneManager.NextTurn();
  }
  public void PrevTurn()
  {
    replaySceneManager.PrevTurn();
  }
}
