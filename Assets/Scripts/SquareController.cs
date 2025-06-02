using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SquareController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer numberSprite;
    [SerializeField] private SpriteRenderer colorTopSprite;
    [SerializeField] private SpriteRenderer colorBottomSprite;
    
    [Header("Values")]
    [SerializeField] private Color readyToReserveColor;
    
    private Quaternion _openedRot;

    public enum SquareStateType
    {
        Empty,
        KingReserved,
        QueenReserved,
        BothReserved,
        KingOccupied,
        QueenOccupied,
    }
    
    public int Id {get; private set;}
    public bool IsOpened { get; private set; }
    public bool IsReadyToReserve { get; private set; }
    public SquareStateType State { get; private set; }

    private void Awake()
    {
        _openedRot = Quaternion.Euler(0, 180, 0);
    }

    public void Init(int id, Sprite number)
    {
        Id = id;
        IsOpened = false;
        IsReadyToReserve = false;
        transform.rotation = Quaternion.identity;
        State = SquareStateType.Empty;
        numberSprite.sprite = number;
        numberSprite.color = Color.white;
        colorTopSprite.color = Color.clear;
        colorBottomSprite.color = Color.clear;
    }

    private void OnMouseDown()
    {
        if (IsOpened) return;
        MainSceneManager.Instance.SquareSelected(Id);
    }

    public void ToggleReadyToReserve()
    {
        if (IsReadyToReserve)
        {
            IsReadyToReserve = false;
            numberSprite.color = Color.white;
        }
        else
        {
            IsReadyToReserve = true;
            numberSprite.color = readyToReserveColor;
        }
    }

    public void Reserve(int player, Color color)
    {
        switch (player)
        {
            case 0: // King
                if (State != SquareStateType.Empty)
                {
                    Debug.LogError($"이상한 reserve: " + State);
                    return;
                }

                State = SquareStateType.KingReserved;
                colorTopSprite.color = color;
                colorBottomSprite.color = color;
                break;
            
            case 1: // Queen
                if (State == SquareStateType.Empty)
                {
                    State = SquareStateType.QueenReserved;
                    colorTopSprite.color = color;
                }
                else if (State == SquareStateType.KingReserved)
                {
                    State = SquareStateType.BothReserved;
                }
                else
                {
                    Debug.LogError($"이상한 reserve: " + State);
                    return;
                }
                
                colorBottomSprite.color = color;
                break;
            
            default:
                Debug.LogError("이상한 플레이어: " + player);
                return;
        }

        IsReadyToReserve = false;
        numberSprite.color = Color.white;
    }

    public void Open(int player, Color color)
    {
        
    }
}
