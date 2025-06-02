using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBtnBehaviour : MonoBehaviour
{
    private enum PushDirectionType
    {
        ColumnDown,
        ColumnUp,
        RowDown,
        RowUp,
    }
    
    private PushDirectionType _pushDirection;
    private int _id;

    public void Init(int pushDirection, int id)
    {
        _pushDirection = (PushDirectionType)pushDirection;
        _id = id;
    }

    public void PushBtnClicked()
    {
        MainSceneManager.Instance.Push((int)_pushDirection, _id);
    }
}
