using UnityEngine;
using UnityEngine.EventSystems;

public class SquareController : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private SpriteRenderer numberSprite;

    private int _id;

    public enum SquareStateType
    {
        Empty,
        KingReserved,
        QueenReserved,
        BothReserved,
        KingOccupied,
        QueenOccupied,
    }
    
    public SquareStateType State { get; private set; }

    public void Init(int id, Sprite numberSprite)
    {
        _id = id;
        State = SquareStateType.Empty;
        this.numberSprite.sprite = numberSprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
