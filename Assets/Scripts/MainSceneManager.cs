using System;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneManager : MonoBehaviour
{
    public static MainSceneManager Instance { get; private set; }
    
    [Header("References")] 
    [SerializeField] private Transform[] columns;
    [SerializeField] private GameObject kingReserveBtn, queenReserveBtn;
    
    [Header("Values")] 
    [SerializeField] private int reservedSquareCnt;
    [SerializeField] private Color kingColor, queenColor;

    private Dictionary<int, Dictionary<int, SquareController>> _squareControllerDict;
    private int _squareLength;
    private int _readyToReserveCnt;
    private Sprite[] _numberSprites;

    private enum PlayerType
    {
        King = 0,
        Queen = 1,
    }

    private enum GameStateType
    {
        Reserve,
        Playing,
        KingWin,
        QueenWin,
        Draw,
    }

    private PlayerType _currTurn;
    private GameStateType _currState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Init()
    {
        _currTurn = PlayerType.King;
        _currState = GameStateType.Reserve;
        _numberSprites = Resources.LoadAll<Sprite>("Numbers");
        
        // 각각의 square 초기화
        _squareControllerDict = new Dictionary<int, Dictionary<int, SquareController>>();
        _squareLength = columns.Length;
        
        for (int x = 0; x < _squareLength; x++)
        {
            _squareControllerDict.Add(x, new Dictionary<int, SquareController>());
            
            for (int y = 0; y < _squareLength; y++)
            {
                int id = x * columns.Length + y;
                
                SquareController controller = columns[x].GetChild(y).gameObject.GetComponent<SquareController>();
                controller.Init(id, _numberSprites[id]);
                _squareControllerDict[x].Add(y, controller);
            }
        }
        
        // Reserve 초기화
        _readyToReserveCnt = 0;
        kingReserveBtn.gameObject.SetActive(true);
        queenReserveBtn.gameObject.SetActive(false);
    }

    public void RunGame()
    {
        switch (_currState)
        {
            case GameStateType.Reserve:
                ReserveSquare();
                break;
            
            case GameStateType.Playing:
                bool isGameEnded = CheckWinner();
                if (isGameEnded) EndGame();
                else NextTurn();
                break;
        }
    }

    public void ReserveSquare()
    {
        if (_currTurn == PlayerType.King)
        {
            for (int x = 0; x < _squareLength; x++)
            for (int y = 0; y < _squareLength; y++)
                if (_squareControllerDict[x][y].IsReadyToReserve)
                    _squareControllerDict[x][y].Reserve(0, kingColor);

            _readyToReserveCnt = 0;
            _currTurn = PlayerType.Queen;
            kingReserveBtn.gameObject.SetActive(false);
            queenReserveBtn.gameObject.SetActive(true);
        }
        else if (_currTurn == PlayerType.Queen)
        {
            for (int x = 0; x < _squareLength; x++)
            for (int y = 0; y < _squareLength; y++)
                if (_squareControllerDict[x][y].IsReadyToReserve)
                    _squareControllerDict[x][y].Reserve(1, queenColor);
            
            _readyToReserveCnt = 0;
            _currTurn = PlayerType.King;
            _currState = GameStateType.Playing;
            kingReserveBtn.gameObject.SetActive(false);
            queenReserveBtn.gameObject.SetActive(false);
        }
    }

    private void NextTurn()
    {
        CheckWinner();

        if (_currTurn == PlayerType.King)
        {
            _currTurn = PlayerType.Queen;
        }
        else if (_currTurn == PlayerType.Queen)
        {
            _currTurn = PlayerType.King;
        }
    }

    private bool CheckWinner()
    {
        return false;
    }

    private void EndGame()
    {
        
    }

    public void SquareSelected(int id)
    {
        switch (_currState)
        {
            case GameStateType.Reserve:
                SquareController squareController = _squareControllerDict[id / _squareLength][id % _squareLength];
                if(squareController.IsReadyToReserve) _readyToReserveCnt--;
                else if (_readyToReserveCnt >= reservedSquareCnt) return;
                else _readyToReserveCnt++;
                
                squareController.ToggleReadyToReserve();
                break;
            
            case GameStateType.Playing:
                break;
        }
    }
}
