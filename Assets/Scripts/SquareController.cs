using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class SquareController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer numberSpriteRenderer;
    [SerializeField] private SpriteRenderer markSpriteRenderer;
    
    [Header("Values")]
    [SerializeField] private float moveAnimationPosZ;
    [SerializeField] private float openAnimationDuration, colorChangeDuration;
    [SerializeField] private float moveAnimationDuration, moveZAxisAnimationDuration;
    
    [Header("Sprites")]
    [SerializeField] private Sprite[] iconSprites;
    [SerializeField] private Sprite[] markSprites;
    public Sprite NumberSprite { get; private set; }
    
    private readonly Vector3 _openedRot = new Vector3(0, 180, 0);
    
    public Sequence MovingSequence { get; private set; }

    public enum SquareStateType
    {
        Empty,
        ZombieReserved,
        VaccineReserved,
        BothReserved,
        ZombieOccupied,
        VaccineOccupied,
    }
    
    public bool IsOpened { get; private set; }
    public bool IsReadyToReserve { get; private set; }
    public SquareStateType State { get; private set; }

    private readonly Vector3 _numberScale = new Vector3(0.07f, 0.07f, 1f), _iconScale = new Vector3(0.08f, 0.08f, 1f);

    public void Init(Sprite number, Vector3 initPos)
    {
        IsOpened = false;
        IsReadyToReserve = false;
        transform.rotation = Quaternion.identity;
        State = SquareStateType.Empty;
        transform.position = initPos;
        numberSpriteRenderer.sprite = number;

        NumberSprite = number;
        numberSpriteRenderer.gameObject.transform.localScale = _numberScale;
        markSpriteRenderer.sprite = null;
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
            numberSpriteRenderer.sprite = NumberSprite;
            numberSpriteRenderer.gameObject.transform.localScale = _numberScale;
        }
        else
        {
            IsReadyToReserve = true;
            numberSpriteRenderer.sprite = iconSprites[player];
            numberSpriteRenderer.gameObject.transform.localScale = _iconScale;
        }
    }

    public void Reserve(int player)
    {
        switch (player)
        {
            case 0: // Zombie
                if (State != SquareStateType.Empty)
                {
                    Debug.LogError($"이상한 reserve: " + State);
                    return;
                }

                State = SquareStateType.ZombieReserved;
                markSpriteRenderer.sprite = iconSprites[0];
                break;
            
            case 1: // Vaccine
                if (State == SquareStateType.Empty)
                {
                    State = SquareStateType.VaccineReserved;
                    markSpriteRenderer.sprite = iconSprites[1];
                }
                else if (State == SquareStateType.ZombieReserved)
                {
                    State = SquareStateType.BothReserved;
                    markSpriteRenderer.sprite = iconSprites[2];
                }
                else
                {
                    Debug.LogError($"이상한 reserve: " + State);
                    return;
                }
                break;
            
            default:
                Debug.LogError("이상한 플레이어: " + player);
                return;
        }

        IsReadyToReserve = false;
        numberSpriteRenderer.sprite = NumberSprite;
        numberSpriteRenderer.gameObject.transform.localScale = _numberScale;
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
                State = player == 0 ? SquareStateType.ZombieOccupied : SquareStateType.VaccineOccupied;
                markSpriteRenderer.color = Color.clear;
                markSpriteRenderer.sprite = markSprites[player];
                MovingSequence.Append(markSpriteRenderer.DOColor(Color.white, colorChangeDuration));
                break;
            
            case SquareStateType.ZombieReserved:
                State = SquareStateType.ZombieOccupied;
                MovingSequence.Append(markSpriteRenderer.DOColor(Color.clear, colorChangeDuration / 2))
                    .AppendCallback(() => markSpriteRenderer.sprite = markSprites[0])
                    .Append(markSpriteRenderer.DOColor(Color.white, colorChangeDuration / 2));
                break;
            
            case SquareStateType.VaccineReserved:
                State = SquareStateType.VaccineOccupied;
                MovingSequence.Append(markSpriteRenderer.DOColor(Color.clear, colorChangeDuration / 2))
                    .AppendCallback(() => markSpriteRenderer.sprite = markSprites[1])
                    .Append(markSpriteRenderer.DOColor(Color.white, colorChangeDuration / 2));
                break;
            
            case SquareStateType.BothReserved:
                State = player == 0 ? SquareStateType.VaccineOccupied : SquareStateType.ZombieOccupied;
                int occupier = Mathf.Abs(player - 1);
                MovingSequence.Append(markSpriteRenderer.DOColor(Color.clear, colorChangeDuration / 2))
                    .AppendCallback(() => markSpriteRenderer.sprite = markSprites[occupier])
                    .Append(markSpriteRenderer.DOColor(Color.white, colorChangeDuration / 2));
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
            // MovingSequence.OnComplete(()=>MainSceneManager.Instance.RunGame());
            MovingSequence.Play().OnComplete(()=>MainSceneManager.Instance.PushEnded());
        }
        else
        {
            MovingSequence.Append(transform.DOMove(newPos, moveAnimationDuration));
            MovingSequence.Play();
        }
    }
}
