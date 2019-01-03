using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplayControlPanelController : MonoBehaviour
{
  public ReplaySceneManager replaySceneManager;

  public Slider slider;
  public TMP_Text turnText;
  public TMP_Text gameLengthText;
  public Button playButton;
  public Button nextButton;
  public Sprite playSprite;
  public Sprite pauseSprite;
  public TMP_Text speedText;
  public Button collapseButton;
  private RectTransform rectTransform;
  private bool isAutoplaying = false;
  private bool isAnimating = false;

  private void Awake()
  {
    rectTransform = transform as RectTransform;
  }
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

  public void UpdateAutoplayState(bool isAutoplaying)
  {
    this.isAutoplaying = isAutoplaying;
    playButton.image.sprite = isAutoplaying ? pauseSprite : playSprite;
    UpdateNextButtonState();
  }

  public void UpdateAnimatingState(bool isAnimating)
  {
    this.isAnimating = isAnimating;
    UpdateNextButtonState();
  }

  void UpdateNextButtonState()
  {
    nextButton.interactable = !isAutoplaying && !isAnimating;
  }
  public void OnSliderMoved(float value)
  {
    replaySceneManager.GoToTurn((int)value);
  }

  public void OnPlayOrPauseClicked()
  {
    replaySceneManager.PlayOrPause();
  }
  public void OnNextClicked()
  {
    replaySceneManager.NextTurn();
  }
  public void OnPrevClicked()
  {
    replaySceneManager.PrevTurn();
  }

  public void OnIncreaseSpeedClicked()
  {
    replaySceneManager.IncreaseSpeed();
  }
  public void OnDecreaseSpeedClicked()
  {
    replaySceneManager.DecreaseSpeed();
  }
  public void UpdateSpeed(float speed)
  {
    speedText.text = $"{speed.ToString()}x";
  }

  public void OnCollapseButtonClick()
  {
    float targetY = collapseButton.transform.localScale.y > 0 ? 0 : ((collapseButton.transform as RectTransform).sizeDelta.y - rectTransform.sizeDelta.y);
    collapseButton.transform.localScale = Vector3.one.WithY(-collapseButton.transform.localScale.y);
    collapseButton.interactable = false;
    rectTransform.DOAnchorPosY(targetY, 0.3f).OnComplete(() => collapseButton.interactable = true);
  }
}
