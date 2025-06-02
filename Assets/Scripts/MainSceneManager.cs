using System;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneManager : MonoBehaviour
{
    public static MainSceneManager Instance { get; private set; }
    
    [Header("References")] 
    [SerializeField] private Transform[] columns;
    [SerializeField] private GameObject kingReserveBtn, queenReserveBtn;
    [SerializeField] private GameObject pushColumnDownParent, pushColumnUpParent, pushRowDownParent, pushRowUpParent;
    
    [Header("Values")] 
    [SerializeField] private int reservedSquareCnt;
    [SerializeField] private Color kingColor, queenColor;

    private Dictionary<int, Dictionary<int, SquareController>> _squareControllerDict;
    private int _squareLength;
    private int _readyToReserveCnt;
    private Sprite[] _numberSprites;
    private float _squareHalfSize;

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
                squaresControllers[y].Init(id, _numberSprites[id], kingColor, queenColor, initPos);
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
        switch (_currState)
        {
            case GameStateType.Reserve:
                ReserveSquare();
                break;
            
            case GameStateType.Playing:
                bool isGameEnded = CheckWinner();
                if (isGameEnded) EndGame();
                else _currTurn = _currTurn == PlayerType.King ? PlayerType.Queen : PlayerType.King;
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
        return false;
    }

    private void EndGame()
    {
        
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
                squareController.Open(_currTurn == PlayerType.King ? 0 : 1);
                RunGame();
                break;
        }
    }

    public void Push(int pushDirection, int id)
    {
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
                }
                newPos.y = ((float)_squareLength * -1 + 1) * _squareHalfSize;
                tmp.Push(true, newPos);
                _squareControllerDict[id][_squareLength - 1] = tmp;
                break;
            
            case 1: // Column Up
                tmp = _squareControllerDict[id][_squareLength - 1];
                newPos = new Vector3(((float)_squareLength * -1 + 2 * id + 1) * _squareHalfSize, 0, 0);
                for (int y = _squareLength - 1; y > 0; y--)
                {
                    newPos.y = ((float)_squareLength - 2 * y - 1) * _squareHalfSize;
                    _squareControllerDict[id][y - 1].Push(false, newPos);
                    _squareControllerDict[id][y] = _squareControllerDict[id][y - 1];
                }
                newPos.y = ((float)_squareLength - 1) * _squareHalfSize;
                tmp.Push(true, newPos);
                _squareControllerDict[id][0] = tmp;
                break;
            
            case 2: // Row Down
                tmp = _squareControllerDict[0][id];
                newPos = new Vector3(0, ((float)_squareLength - 2 * id - 1) * _squareHalfSize, 0);
                for (int x = 0; x < _squareLength-1; x++)
                {
                    newPos.x = ((float)_squareLength * -1 + 2 * x + 1) * _squareHalfSize;
                    _squareControllerDict[x+1][id].Push(false, newPos);
                    _squareControllerDict[x][id] = _squareControllerDict[x+1][id];
                }
                newPos.x = ((float)_squareLength - 1) * _squareHalfSize;
                tmp.Push(true, newPos);
                _squareControllerDict[_squareLength-1][id] = tmp;
                break;
            
            case 3: // Row Up
                tmp = _squareControllerDict[_squareLength-1][id];
                newPos = new Vector3(0, ((float)_squareLength - 2 * id - 1) * _squareHalfSize, 0);
                for (int x = _squareLength - 1; x > 0; x--)
                {
                    newPos.x = ((float)_squareLength * -1 + 2 * x + 1) * _squareHalfSize;
                    _squareControllerDict[x-1][id].Push(false, newPos);
                    _squareControllerDict[x][id] = _squareControllerDict[x-1][id];
                }
                newPos.x = ((float)_squareLength * -1 + 1) * _squareHalfSize;
                tmp.Push(true, newPos);
                _squareControllerDict[0][id] = tmp;
                break;
        }
        
        RunGame();
    }
}
