using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySFX("Hover");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
    }
}