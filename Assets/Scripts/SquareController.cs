using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class SquareController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer numberSprite;
    [SerializeField] private SpriteRenderer colorTopSprite;
    [SerializeField] private SpriteRenderer colorBottomSprite;
    
    [Header("Values")]
    [SerializeField] private float moveAnimationPosZ;
    [SerializeField] private float openAnimationDuration, colorChangeDuration;
    [SerializeField] private float moveAnimationDuration, moveZAxisAnimationDuration;
    
    private Vector3 _openedRot = new Vector3(0, 180, 0);

    private Color[] _playerColors = new Color[2];
    
    public Sequence MovingSequence { get; private set; }

    public enum SquareStateType
    {
        Empty,
        KingReserved,
        QueenReserved,
        BothReserved,
        KingOccupied,
        QueenOccupied,
    }
    
    public bool IsOpened { get; private set; }
    public bool IsReadyToReserve { get; private set; }
    public SquareStateType State { get; private set; }

    public void Init(Sprite number, Color kingColor, Color queenColor, Vector3 initPos)
    {
        IsOpened = false;
        IsReadyToReserve = false;
        transform.rotation = Quaternion.identity;
        State = SquareStateType.Empty;
        transform.position = initPos;
        numberSprite.sprite = number;
        _playerColors[0] = kingColor;
        _playerColors[1] = queenColor;
        numberSprite.color = Color.white;
        colorTopSprite.color = Color.clear;
        colorBottomSprite.color = Color.clear;
    }

    private void OnMouseDown()
    {
        if (IsOpened) return;
        MainSceneManager.Instance.SquareSelected(this);
    }

    public void ToggleReadyToReserve(int player)
    {
        if (IsReadyToReserve)
        {
            IsReadyToReserve = false;
            numberSprite.color = Color.white;
        }
        else
        {
            IsReadyToReserve = true;
            numberSprite.color = _playerColors[player];
        }
    }

    public void Reserve(int player)
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
                colorTopSprite.color = _playerColors[player];
                colorBottomSprite.color = _playerColors[player];
                break;
            
            case 1: // Queen
                if (State == SquareStateType.Empty)
                {
                    State = SquareStateType.QueenReserved;
                    colorTopSprite.color = _playerColors[player];
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
                
                colorBottomSprite.color = _playerColors[player];
                break;
            
            default:
                Debug.LogError("이상한 플레이어: " + player);
                return;
        }

        IsReadyToReserve = false;
        numberSprite.color = Color.white;
    }

    public void Open(int player)
    {
        if (MovingSequence != null &&MovingSequence.IsActive())
            MovingSequence.Complete();
        MovingSequence = DOTween.Sequence();
        
        IsOpened = true;

        switch (State)
        {
            case SquareStateType.Empty:
                State = player == 0 ? SquareStateType.KingOccupied : SquareStateType.QueenOccupied;
                MovingSequence.Append(colorTopSprite.DOColor(_playerColors[player], colorChangeDuration))
                    .Join(colorBottomSprite.DOColor(_playerColors[player], colorChangeDuration));
                break;
            
            case SquareStateType.KingReserved:
                State = SquareStateType.KingOccupied;
                break;
            
            case SquareStateType.QueenReserved:
                State = SquareStateType.QueenOccupied;
                break;
            
            case SquareStateType.BothReserved:
                State = player == 0 ? SquareStateType.QueenOccupied : SquareStateType.KingOccupied;
                int occupier = Mathf.Abs(player - 1);
                MovingSequence.Append(colorTopSprite.DOColor(_playerColors[occupier], colorChangeDuration))
                    .Join(colorBottomSprite.DOColor(_playerColors[occupier], colorChangeDuration));
                break;
            
            default:
                Debug.LogError("이상한 상태: " + State);
                return;
        }
        
        MovingSequence.Prepend(transform.DORotate(_openedRot, openAnimationDuration));
        MovingSequence.Play().OnComplete(()=>MainSceneManager.Instance.isMoving=false);
    }

    public void Push(bool isLastOne, Vector3 newPos)
    {
        if (MovingSequence != null &&MovingSequence.IsActive())
            MovingSequence.Complete();
        MovingSequence = DOTween.Sequence();

        if (isLastOne)
        {
            Vector3 milestone0 = new Vector3(transform.position.x, transform.position.y, moveAnimationPosZ);
            Vector3 milestone1 = new Vector3(newPos.x, newPos.y, moveAnimationPosZ);
            MovingSequence.Append(transform.DOMove(milestone0, moveZAxisAnimationDuration));
            MovingSequence.Append(transform.DOMove(milestone1, moveAnimationDuration));
            MovingSequence.Append(transform.DOMove(newPos, moveZAxisAnimationDuration));
            // MovingSequence.Play().OnComplete(()=>MainSceneManager.Instance.isMoving=false);
            MovingSequence.Play().OnComplete(()=>MainSceneManager.Instance.RunGame());
        }
        else
        {
            MovingSequence.Append(transform.DOMove(newPos, moveAnimationDuration));
            MovingSequence.Play();
        }
    }
}
