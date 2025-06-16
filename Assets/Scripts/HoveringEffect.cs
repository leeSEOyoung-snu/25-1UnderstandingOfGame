using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoveringEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("TMP Color")] 
    [SerializeField] private bool changeColor;
    [SerializeField] private TextMeshProUGUI targetTmp;
    [SerializeField] private Color originalColor, hoveredColor;

    public void Init()
    {
        if (changeColor) targetTmp.color = originalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (changeColor) targetTmp.color = hoveredColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (changeColor) targetTmp.color = originalColor;
    }
}
