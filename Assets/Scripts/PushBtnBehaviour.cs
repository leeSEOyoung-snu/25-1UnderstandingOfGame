using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PushBtnBehaviour : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Button pushButton;
    [SerializeField] private Image buttonImg;
    [SerializeField] private Sprite originalSprite, disabledSprite;
    
    private enum PushDirectionType
    {
        ColumnDown,
        ColumnUp,
        RowDown,
        RowUp,
    }
    
    private PushDirectionType _pushDirection;
    private int _id;

    private int _disabledTurn;

    public void Init(int pushDirection, int id)
    {
        _pushDirection = (PushDirectionType)pushDirection;
        _id = id;
        _disabledTurn = 0;
        buttonText.text = "";
        pushButton.interactable = true;
        buttonImg.sprite = originalSprite;
    }

    public void PushBtnClicked()
    {
        MainSceneManager.Instance.Push((int)_pushDirection, _id);
    }

    public void MakeDisabled(int turn)
    {
        _disabledTurn = turn+1;
        pushButton.interactable = false;
        buttonText.text = turn.ToString();
        buttonImg.sprite = disabledSprite;
    }

    public void UpdateDisabled()
    {
        if (_disabledTurn == 0) return;
        _disabledTurn--;
        buttonText.text = _disabledTurn.ToString();
        if (_disabledTurn == 0)
        {
            pushButton.interactable = true;
            buttonText.text = "";
            buttonImg.sprite = originalSprite;
        }
    }
}
