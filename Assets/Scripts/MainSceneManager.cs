using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainSceneManager : MonoBehaviour
{
    public static MainSceneManager Instance { get; private set; }
    
    [Header("References")] 
    [SerializeField] private Transform[] columns;
    [SerializeField] private GameObject kingReserveBtn, queenReserveBtn;
    [SerializeField] private GameObject pushColumnDownParent, pushColumnUpParent, pushRowDownParent, pushRowUpParent;
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private TextMeshProUGUI endingText;
    
    [Header("Values")] 
    [SerializeField] private int reservedSquareCnt;
    [SerializeField] private int winCondition;
    [SerializeField] private Color kingColor, queenColor;

    private Dictionary<int, Dictionary<int, SquareController>> _squareControllerDict;
    private int _squareLength;
    private int _readyToReserveCnt;
    private Sprite[] _numberSprites;
    private float _squareHalfSize;

    public bool isMoving;

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
        isMoving = false;
        
        _currTurn = PlayerType.King;
        _currState = GameStateType.Reserve;
        _numberSprites = Resources.LoadAll<Sprite>("Numbers");
        endingPanel.SetActive(false);
        
        // square controller 및 square 위치 초기화
        _squareControllerDict = new Dictionary<int, Dictionary<int, SquareController>>();
        _squareLength = columns.Length;
        _squareHalfSize = columns[0].GetChild(0).transform.localScale.x / 2;
        
        for (int x = 0; x < _squareLength; x++)
        {
            _squareControllerDict.Add(x, new Dictionary<int, SquareController>());
            SquareController[] squaresControllers = columns[x].GetComponentsInChildren<SquareController>();
            
            for (int y = 0; y < _squareLength; y++)
            {
                int id = x * columns.Length + y;
                
                float initPosX = ((float)_squareLength*-1 + 2*x + 1) * _squareHalfSize;
                float initPosY = ((float)_squareLength - 2*y - 1) * _squareHalfSize;
                Vector3 initPos = new Vector3(initPosX, initPosY, 0);
                squaresControllers[y].Init(_numberSprites[id], kingColor, queenColor, initPos);
                _squareControllerDict[x].Add(y, squaresControllers[y]);
            }
        }
        
        // Reserve 초기화
        _readyToReserveCnt = 0;
        kingReserveBtn.gameObject.SetActive(true);
        queenReserveBtn.gameObject.SetActive(false);
        
        // Push 초기화
        pushColumnDownParent.SetActive(false);
        pushColumnUpParent.SetActive(false);
        pushRowDownParent.SetActive(false);
        pushRowUpParent.SetActive(false);
        
        PushBtnBehaviour[] pushBtnsBehaviours = pushColumnDownParent.GetComponentsInChildren<PushBtnBehaviour>();
        for(int i = 0; i < pushBtnsBehaviours.Length; i++)
            pushBtnsBehaviours[i].Init(0, i);
        
        pushBtnsBehaviours = pushColumnUpParent.GetComponentsInChildren<PushBtnBehaviour>();
        for(int i = 0; i < pushBtnsBehaviours.Length; i++)
            pushBtnsBehaviours[i].Init(1, i);
        
        pushBtnsBehaviours = pushRowDownParent.GetComponentsInChildren<PushBtnBehaviour>();
        for(int i = 0; i < pushBtnsBehaviours.Length; i++)
            pushBtnsBehaviours[i].Init(2, i);
        
        pushBtnsBehaviours = pushRowUpParent.GetComponentsInChildren<PushBtnBehaviour>();
        for(int i = 0; i < pushBtnsBehaviours.Length; i++)
            pushBtnsBehaviours[i].Init(3, i);
    }

    public void RunGame()
    {
        isMoving = false;
        switch (_currState)
        {
            case GameStateType.Reserve:
                ReserveSquare();
                break;
            
            case GameStateType.Playing:
                bool isGameEnded = CheckWinner();
                if (!isGameEnded) _currTurn = _currTurn == PlayerType.King ? PlayerType.Queen : PlayerType.King;
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
                    _squareControllerDict[x][y].Reserve(0);

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
                    _squareControllerDict[x][y].Reserve(1);
            
            _readyToReserveCnt = 0;
            _currTurn = PlayerType.King;
            _currState = GameStateType.Playing;
            kingReserveBtn.gameObject.SetActive(false);
            queenReserveBtn.gameObject.SetActive(false);
            
            pushColumnDownParent.SetActive(true);
            pushColumnUpParent.SetActive(true);
            pushRowDownParent.SetActive(true);
            pushRowUpParent.SetActive(true);
        }
    }

    private bool CheckWinner()
    {
        bool isDraw = true;
        int maxKingCnt = 0, maxQueenCnt = 0;
        int currKingCnt = 0, currQueenCnt = 0;
        
        // 세로 방향 체크
        for (int x = 0; x < _squareLength; x++)
        {
            for (int y = 0; y < _squareLength; y++)
            {
                switch (_squareControllerDict[x][y].State)
                {
                    case SquareController.SquareStateType.KingOccupied:
                        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
                        currQueenCnt = 0;
                        currKingCnt++;
                        break;
                    
                    case SquareController.SquareStateType.QueenOccupied:
                        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
                        currKingCnt = 0;
                        currQueenCnt++;
                        break;
                    
                    default:
                        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
                        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
                        currQueenCnt = 0;
                        currKingCnt = 0;
                        isDraw = false;
                        break;
                }
            }
            if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
            if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
            currKingCnt = 0;
            currQueenCnt = 0;
        }
        
        // Debug.Log($"세로 체크 - maxKing: {maxKingCnt}, maxQueen: {maxQueenCnt}");
        
        // 가로 방향 체크
        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
        currKingCnt = 0;
        currQueenCnt = 0;
        for (int y = 0; y < _squareLength; y++)
        {
            for (int x = 0; x < _squareLength; x++)
            {
                switch (_squareControllerDict[x][y].State)
                {
                    case SquareController.SquareStateType.KingOccupied:
                        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
                        currQueenCnt = 0;
                        currKingCnt++;
                        break;
                    
                    case SquareController.SquareStateType.QueenOccupied:
                        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
                        currKingCnt = 0;
                        currQueenCnt++;
                        break;
                    default:
                        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
                        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
                        currQueenCnt = 0;
                        currKingCnt = 0;
                        isDraw = false;
                        break;
                }
                // Debug.Log($"square state: {_squareControllerDict[x][y].State}, maxKing: {maxKingCnt}, maxQueen: {maxQueenCnt}");
            }
            if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
            if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
            currKingCnt = 0;
            currQueenCnt = 0;
        }
        
        // Debug.Log($"가로 체크 - maxKing: {maxKingCnt}, maxQueen: {maxQueenCnt}");
        
        // 우하향 대각선 체크
        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
        currKingCnt = 0;
        currQueenCnt = 0;
        for (int x = 0; x < _squareLength; x++)
        {
            for (int i = 0; i < _squareLength - x; i++)
            {
                switch (_squareControllerDict[x+i][i].State)
                {
                    case SquareController.SquareStateType.KingOccupied:
                        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
                        currQueenCnt = 0;
                        currKingCnt++;
                        break;
                    
                    case SquareController.SquareStateType.QueenOccupied:
                        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
                        currKingCnt = 0;
                        currQueenCnt++;
                        break;
                    
                    default:
                        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
                        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
                        currQueenCnt = 0;
                        currKingCnt = 0;
                        isDraw = false;
                        break;
                }
            }
            if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
            if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
            currKingCnt = 0;
            currQueenCnt = 0;
        }
        
        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
        currKingCnt = 0;
        currQueenCnt = 0;
        for (int y = 0; y < _squareLength; y++)
        {
            for (int i = 0; i < _squareLength - y; i++)
            {
                switch (_squareControllerDict[i][y+i].State)
                {
                    case SquareController.SquareStateType.KingOccupied:
                        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
                        currQueenCnt = 0;
                        currKingCnt++;
                        break;
                    
                    case SquareController.SquareStateType.QueenOccupied:
                        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
                        currKingCnt = 0;
                        currQueenCnt++;
                        break;
                    
                    default:
                        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
                        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
                        currQueenCnt = 0;
                        currKingCnt = 0;
                        isDraw = false;
                        break;
                }
            }
            if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
            if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
            currKingCnt = 0;
            currQueenCnt = 0;
        }
        
        // Debug.Log($"우하향 체크 - maxKing: {maxKingCnt}, maxQueen: {maxQueenCnt}");
        
        // 우상향 대각선 체크
        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
        currKingCnt = 0;
        currQueenCnt = 0;
        for (int x = 0; x < _squareLength; x++)
        {
            for (int i = 0; i < x+1; i++)
            {
                switch (_squareControllerDict[x-i][i].State)
                {
                    case SquareController.SquareStateType.KingOccupied:
                        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
                        currQueenCnt = 0;
                        currKingCnt++;
                        break;
                    
                    case SquareController.SquareStateType.QueenOccupied:
                        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
                        currKingCnt = 0;
                        currQueenCnt++;
                        break;
                    
                    default:
                        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
                        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
                        currQueenCnt = 0;
                        currKingCnt = 0;
                        isDraw = false;
                        break;
                }
            }
            if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
            if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
            currKingCnt = 0;
            currQueenCnt = 0;
        }
        
        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
        currKingCnt = 0;
        currQueenCnt = 0;
        for (int y = 0; y < _squareLength; y++)
        {
            for (int i = 0; i < y+1; i++)
            {
                switch (_squareControllerDict[i][y-i].State)
                {
                    case SquareController.SquareStateType.KingOccupied:
                        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
                        currQueenCnt = 0;
                        currKingCnt++;
                        break;
                    
                    case SquareController.SquareStateType.QueenOccupied:
                        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
                        currKingCnt = 0;
                        currQueenCnt++;
                        break;
                    
                    default:
                        if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
                        if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
                        currQueenCnt = 0;
                        currKingCnt = 0;
                        isDraw = false;
                        break;
                }
            }
            if (maxQueenCnt < currQueenCnt) maxQueenCnt = currQueenCnt;
            if (maxKingCnt < currKingCnt) maxKingCnt = currKingCnt;
            currKingCnt = 0;
            currQueenCnt = 0;
        }
        
        Debug.Log($"maxKing: {maxKingCnt}, maxQueen: {maxQueenCnt}");
        
        if (maxKingCnt >= winCondition) _currState = GameStateType.KingWin;
        else if (maxQueenCnt >= winCondition) _currState = GameStateType.QueenWin;
        else if (isDraw) _currState = GameStateType.Draw; 
        else return false;
        
        EndGame();
        return true;
    }

    private void EndGame()
    {
        switch (_currState)
        {
            case GameStateType.KingWin:
                endingText.text = "King Win!";
                break;
            
            case GameStateType.QueenWin:
                endingText.text = "Queen Win!";
                break;
            
            case GameStateType.Draw:
                endingText.text = "Draw";
                break;
            
            default:
                Debug.LogError("이상한 상태: " + _currState);
                return;
        }
        
        endingPanel.SetActive(true);
    }

    public void SquareSelected(SquareController squareController)
    {
        switch (_currState)
        {
            case GameStateType.Reserve:
                if(squareController.IsReadyToReserve) _readyToReserveCnt--;
                else if (_readyToReserveCnt >= reservedSquareCnt) return;
                else _readyToReserveCnt++;
                
                squareController.ToggleReadyToReserve(_currTurn == PlayerType.King ? 0 : 1);
                break;
            
            case GameStateType.Playing:
                if (isMoving) return;
                isMoving = true;
                squareController.Open(_currTurn == PlayerType.King ? 0 : 1);
                RunGame();
                break;
        }
    }

    public void Push(int pushDirection, int id)
    {
        if (isMoving) return;
        isMoving = true;
        SquareController tmp;
        Vector3 newPos;
        switch(pushDirection) 
        {
            case 0: // Column Down
                tmp = _squareControllerDict[id][0];
                newPos = new Vector3(((float)_squareLength * -1 + 2 * id + 1) * _squareHalfSize, 0, 0);
                for (int y = 0; y < _squareLength-1; y++)
                {
                    newPos.y = ((float)_squareLength - 2 * y - 1) * _squareHalfSize;
                    _squareControllerDict[id][y+1].Push(false, newPos);
                    _squareControllerDict[id][y] = _squareControllerDict[id][y+1];
                    // Debug.Log($"Column Down - y: {y}");
                }
                newPos.y = ((float)_squareLength * -1 + 1) * _squareHalfSize;
                tmp.Push(true, newPos);
                _squareControllerDict[id][_squareLength - 1] = tmp;
                // Debug.Log($"Column Down - y: 5");
                break;
            
            case 1: // Column Up
                tmp = _squareControllerDict[id][_squareLength - 1];
                newPos = new Vector3(((float)_squareLength * -1 + 2 * id + 1) * _squareHalfSize, 0, 0);
                for (int y = _squareLength - 1; y > 0; y--)
                {
                    newPos.y = ((float)_squareLength - 2 * y - 1) * _squareHalfSize;
                    _squareControllerDict[id][y - 1].Push(false, newPos);
                    _squareControllerDict[id][y] = _squareControllerDict[id][y - 1];
                    // Debug.Log($"Column Up - y: {y}");
                }
                newPos.y = ((float)_squareLength - 1) * _squareHalfSize;
                tmp.Push(true, newPos);
                _squareControllerDict[id][0] = tmp;
                // Debug.Log($"Column Up - y: 0");
                break;
            
            case 2: // Row Down
                tmp = _squareControllerDict[0][id];
                newPos = new Vector3(0, ((float)_squareLength - 2 * id - 1) * _squareHalfSize, 0);
                for (int x = 0; x < _squareLength-1; x++)
                {
                    newPos.x = ((float)_squareLength * -1 + 2 * x + 1) * _squareHalfSize;
                    _squareControllerDict[x+1][id].Push(false, newPos);
                    _squareControllerDict[x][id] = _squareControllerDict[x+1][id];
                    // Debug.Log($"Row Down - x: {x}");
                }
                newPos.x = ((float)_squareLength - 1) * _squareHalfSize;
                tmp.Push(true, newPos);
                _squareControllerDict[_squareLength-1][id] = tmp;
                // Debug.Log($"Row Down - x: 5");
                break;
            
            case 3: // Row Up
                tmp = _squareControllerDict[_squareLength-1][id];
                newPos = new Vector3(0, ((float)_squareLength - 2 * id - 1) * _squareHalfSize, 0);
                for (int x = _squareLength - 1; x > 0; x--)
                {
                    newPos.x = ((float)_squareLength * -1 + 2 * x + 1) * _squareHalfSize;
                    _squareControllerDict[x-1][id].Push(false, newPos);
                    _squareControllerDict[x][id] = _squareControllerDict[x-1][id];
                    // Debug.Log($"Row Up - x: {x}");
                }
                newPos.x = ((float)_squareLength * -1 + 1) * _squareHalfSize;
                tmp.Push(true, newPos);
                _squareControllerDict[0][id] = tmp;
                // Debug.Log($"Row Up - x: 0");
                break;
        }
    }
    
    
}
