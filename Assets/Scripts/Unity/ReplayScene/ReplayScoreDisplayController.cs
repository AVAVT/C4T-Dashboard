using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReplayScoreDisplayController : MonoBehaviour
{
  public TMP_Text redScore;
  public TMP_Text blueScore;
  public RectTransform influenceBar;
  public RectTransform influenceFill;

  public void VisualizeState(ServerGameState gameState)
  {
    redScore.text = gameState.redScore.ToString();
    blueScore.text = gameState.blueScore.ToString();
    if (gameState.redScore == 0 && gameState.blueScore == 0) UpdateInfluenceBar(0.5f);
    else UpdateInfluenceBar((float)gameState.redScore / (gameState.redScore + gameState.blueScore));
  }

  public void UpdateInfluenceBar(float ratio)
  {
    influenceFill.sizeDelta = influenceBar.sizeDelta.WithX(influenceBar.rect.width * ratio);
  }
}
