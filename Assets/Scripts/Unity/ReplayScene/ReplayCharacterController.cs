using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplayCharacterController : MonoBehaviour
{
  public Image image;
  public GameObject disabledIcon;
  private RectTransform rectTransform;
  private float tileSize;
  private Vector2 offset;

  public void Initialize(Sprite sprite, float tileSize, Vector2 offset)
  {
    rectTransform = transform as RectTransform;
    this.tileSize = tileSize;
    this.offset = offset;
    image.sprite = sprite;
    image.SetNativeSize();
    transform.localScale = Vector3.one * tileSize / rectTransform.sizeDelta.x;
  }

  public void UpdateState(Character state)
  {
    disabledIcon.SetActive(state.isScared || state.isCaught);
    rectTransform.anchoredPosition = new Vector2(state.x, -state.y) * tileSize + offset;
  }
}
