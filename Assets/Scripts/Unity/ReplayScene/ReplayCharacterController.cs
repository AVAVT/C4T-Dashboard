using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class ReplayCharacterController : MonoBehaviour
{
  public Image image;
  public GameObject disabledIcon;
  public TMP_Text harvestText;
  private RectTransform rectTransform;
  private float tileSize;
  private Vector2 offset;

  private Tween currentTween;

  public void Initialize(Sprite sprite, float tileSize, Vector2 offset)
  {
    rectTransform = transform as RectTransform;
    this.tileSize = tileSize;
    this.offset = offset;
    image.sprite = sprite;
    image.SetNativeSize();
    transform.localScale = Vector3.one * tileSize / rectTransform.sizeDelta.x;
  }
  public void AnimateState(Character state, TurnAction turnAction, float duration)
  {
    var cSharpDirection = turnAction.actualDirection.ToDirectionVector();
    var unityDirection = new Vector2((int)cSharpDirection.X, -(int)cSharpDirection.Y);

    currentTween = rectTransform.DOAnchorPos(
      rectTransform.anchoredPosition + unityDirection * tileSize,
      duration
    ).OnComplete(() =>
    {
      UpdateState(state);
      currentTween = null;
    });
  }
  public void UpdateState(Character state)
  {
    currentTween?.Kill();
    harvestText.gameObject.SetActive(state.role == Role.Harvester);
    harvestText.text = state.fruitCarrying.ToString();
    disabledIcon.SetActive(state.isScared || state.isCaught);
    rectTransform.anchoredPosition = new Vector2(state.x, -state.y) * tileSize + offset;
  }
}
