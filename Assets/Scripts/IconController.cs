using System;
using UnityEngine;

public class IconController : MonoBehaviour
{
    [SerializeField] private HoveringEffect hoveringEffect;
    [SerializeField] private GameObject warningMessage;
    [SerializeField] private Transform iconParent;
    [SerializeField] private Sprite iconSprite;

    private GameObject[] _icons;
    private int _currIconCnt;
    private Sprite[] _numberSprites;

    private void Awake()
    {
        _numberSprites = Resources.LoadAll<Sprite>("Numbers");
    }

    public void Init()
    {
        hoveringEffect.Init();
        warningMessage.SetActive(false);

        _icons = new GameObject[iconParent.childCount];
        for (int i = 0; i < _icons.Length; i++)
        {
            _icons[i] = iconParent.GetChild(i).gameObject;
            _icons[i].SetActive(true);
        }
        _currIconCnt = _icons.Length - 1;
        
        gameObject.SetActive(false);
    }

    public void ReadyToReserve()
    {
        gameObject.SetActive(true);
    }

    public void ShowWarningMessage()
    {
        warningMessage.SetActive(true);
    }

    public void EndReserve()
    {
        gameObject.SetActive(false);
    }

    public void AddIcon()
    {
        _icons[++_currIconCnt].SetActive(true);
    }

    public void RemoveIcon()
    {
        _icons[_currIconCnt--].SetActive(false);
    }
}
