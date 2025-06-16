using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainSceneManager : MonoBehaviour
{
    public static MainSceneManager Instance { get; private set; }
    
    [Header("References")] 
    [SerializeField] private Transform[] columns;
    [SerializeField] private IconController zombieIconController, vaccineIconController;
    [SerializeField] private GameObject pushColumnDownParent, pushColumnUpParent, pushRowDownParent, pushRowUpParent;
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private TextMeshProUGUI endingText;
    [SerializeField] private GameObject zombieTurn, vaccineTurn;
    
    [Header("Values")] 
    [SerializeField] private float squareOffset;
    [SerializeField] private int reservedSquareCnt;
    [SerializeField] private int winCondition;
    [SerializeField] private Color zombieColor, vaccineColor;
    [SerializeField] private int disablePushBtnTurn;

    private Dictionary<int, Dictionary<int, SquareController>> _squareControllerDict;
    private Dictionary<int, Dictionary<int, PushBtnBehaviour>> _pushBtnBehavioursDict;
    private int _squareLength;
    private int _readyToReserveCnt;
    private Sprite[] _numberSprites;
    private float _squareHalfSize;

    public bool isMoving;

    private enum PlayerType
    {
        Zombie = 0,
        Vaccine = 1,
    }

    private enum GameStateType
    {
        Reserve,
        Playing,
        ZombieWin,
        VaccineWin,
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
        
        _currTurn = PlayerType.Zombie;
        _currState = GameStateType.Reserve;
        _numberSprites = Resources.LoadAll<Sprite>("Numbers");
        endingPanel.SetActive(false);
        
        zombieTurn.SetActive(false);
        vaccineTurn.SetActive(false);
        
        // square controller 및 square 위치 초기화
        _squareControllerDict = new Dictionary<int, Dictionary<int, SquareController>>();
        _squareLength = columns.Length;
        _squareHalfSize = columns[0].GetChild(0).transform.localScale.x / 2 + squareOffset;
        
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
                squaresControllers[y].Init(_numberSprites[id], initPos);
                _squareControllerDict[x].Add(y, squaresControllers[y]);
            }
        }
        
        // Reserve 초기화
        _readyToReserveCnt = 0;
        zombieIconController.Init();
        zombieIconController.ReadyToReserve();
        vaccineIconController.Init();
        
        // Push 초기화
        pushColumnDownParent.SetActive(false);
        pushColumnUpParent.SetActive(false);
        pushRowDownParent.SetActive(false);
        pushRowUpParent.SetActive(false);
        
        _pushBtnBehavioursDict = new Dictionary<int, Dictionary<int, PushBtnBehaviour>>();
        
        PushBtnBehaviour[] pushBtnsBehaviours = pushColumnDownParent.GetComponentsInChildren<PushBtnBehaviour>();
        _pushBtnBehavioursDict.Add(0, new Dictionary<int, PushBtnBehaviour>());
        for(int i = 0; i < pushBtnsBehaviours.Length; i++)
        {
            pushBtnsBehaviours[i].Init(0, i);
            _pushBtnBehavioursDict[0][i] = pushBtnsBehaviours[i];
        }
        
        pushBtnsBehaviours = pushColumnUpParent.GetComponentsInChildren<PushBtnBehaviour>();
        _pushBtnBehavioursDict.Add(1, new Dictionary<int, PushBtnBehaviour>());
        for(int i = 0; i < pushBtnsBehaviours.Length; i++)
        {
            pushBtnsBehaviours[i].Init(1, pushBtnsBehaviours.Length - 1 - i);
            _pushBtnBehavioursDict[1][pushBtnsBehaviours.Length - 1 - i] = pushBtnsBehaviours[i];
        }
        
        pushBtnsBehaviours = pushRowDownParent.GetComponentsInChildren<PushBtnBehaviour>();
        _pushBtnBehavioursDict.Add(2, new Dictionary<int, PushBtnBehaviour>());
        for(int i = 0; i < pushBtnsBehaviours.Length; i++)
        {
            pushBtnsBehaviours[i].Init(2, pushBtnsBehaviours.Length - 1 - i);
            _pushBtnBehavioursDict[2][pushBtnsBehaviours.Length - 1 - i] = pushBtnsBehaviours[i];
        }
        
        pushBtnsBehaviours = pushRowUpParent.GetComponentsInChildren<PushBtnBehaviour>();
        _pushBtnBehavioursDict.Add(3, new Dictionary<int, PushBtnBehaviour>());
        for(int i = 0; i < pushBtnsBehaviours.Length; i++)
        {
            pushBtnsBehaviours[i].Init(3, i);
            _pushBtnBehavioursDict[3][i] = pushBtnsBehaviours[i];
        }
    }

    public void RunGame()
    {
        // isMoving = false;
        switch (_currState)
        {
            case GameStateType.Reserve:
                ReserveSquare();
                break;
            
            case GameStateType.Playing:
                bool isGameEnded = CheckWinner();
                if (!isGameEnded)
                {
                    if (_currTurn == PlayerType.Zombie)
                    {
                        _currTurn = PlayerType.Vaccine;
                        zombieTurn.SetActive(false);
                        vaccineTurn.SetActive(true);
                    }
                    else if (_currTurn == PlayerType.Vaccine)
                    {
                        _currTurn = PlayerType.Zombie;
                        zombieTurn.SetActive(true);
                        vaccineTurn.SetActive(false);
                    }

                    foreach (var pair1 in _pushBtnBehavioursDict)
                    foreach (var pair2 in pair1.Value)
                        pair2.Value.UpdateDisabled();
                }
                break;
        }
    }

    public void ReserveSquare()
    {
        if (_currTurn == PlayerType.Zombie)
        {
            if (_readyToReserveCnt != reservedSquareCnt)
            {
                zombieIconController.ShowWarningMessage();
                AudioManager.PlaySfx(8);
                return;
            }
            for (int x = 0; x < _squareLength; x++)
            for (int y = 0; y < _squareLength; y++)
                if (_squareControllerDict[x][y].IsReadyToReserve)
                    _squareControllerDict[x][y].Reserve(0);

            _readyToReserveCnt = 0;
            _currTurn = PlayerType.Vaccine;
            zombieIconController.EndReserve();
            vaccineIconController.ReadyToReserve();
        }
        else if (_currTurn == PlayerType.Vaccine)
        {
            if (_readyToReserveCnt != reservedSquareCnt)
            {
                vaccineIconController.ShowWarningMessage();
                AudioManager.PlaySfx(8);
                return;
            }
            for (int x = 0; x < _squareLength; x++)
            for (int y = 0; y < _squareLength; y++)
                if (_squareControllerDict[x][y].IsReadyToReserve)
                    _squareControllerDict[x][y].Reserve(1);
            
            _readyToReserveCnt = 0;
            _currTurn = PlayerType.Zombie;
            zombieTurn.SetActive(true);
            vaccineTurn.SetActive(false);
            _currState = GameStateType.Playing;
            vaccineIconController.EndReserve();
            
            pushColumnDownParent.SetActive(true);
            pushColumnUpParent.SetActive(true);
            pushRowDownParent.SetActive(true);
            pushRowUpParent.SetActive(true);
        }
        
        AudioManager.PlaySfx(9);
    }

    private bool CheckWinner()
    {
        int maxZombieCnt = 0, maxVaccineCnt = 0;
        int currZombieCnt = 0, currVaccineCnt = 0;
        
        // 세로 방향 체크
        for (int x = 0; x < _squareLength; x++)
        {
            for (int y = 0; y < _squareLength; y++)
            {
                switch (_squareControllerDict[x][y].State)
                {
                    case SquareController.SquareStateType.ZombieOccupied:
                        if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
                        currVaccineCnt = 0;
                        currZombieCnt++;
                        break;
                    
                    case SquareController.SquareStateType.VaccineOccupied:
                        if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
                        currZombieCnt = 0;
                        currVaccineCnt++;
                        break;
                    
                    default:
                        if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
                        if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
                        currVaccineCnt = 0;
                        currZombieCnt = 0;
                        break;
                }
            }
            if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
            if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
            currZombieCnt = 0;
            currVaccineCnt = 0;
        }
        
        // Debug.Log($"세로 체크 - maxKing: {maxZombieCnt}, maxQueen: {maxVaccineCnt}");
        
        // 가로 방향 체크
        for (int y = 0; y < _squareLength; y++)
        {
            for (int x = 0; x < _squareLength; x++)
            {
                switch (_squareControllerDict[x][y].State)
                {
                    case SquareController.SquareStateType.ZombieOccupied:
                        if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
                        currVaccineCnt = 0;
                        currZombieCnt++;
                        break;
                    
                    case SquareController.SquareStateType.VaccineOccupied:
                        if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
                        currZombieCnt = 0;
                        currVaccineCnt++;
                        break;
                    default:
                        if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
                        if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
                        currVaccineCnt = 0;
                        currZombieCnt = 0;
                        break;
                }
                // Debug.Log($"square state: {_squareControllerDict[x][y].State}, maxKing: {maxZombieCnt}, maxQueen: {maxVaccineCnt}");
            }
            if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
            if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
            currZombieCnt = 0;
            currVaccineCnt = 0;
        }
        
        // Debug.Log($"가로 체크 - maxKing: {maxZombieCnt}, maxQueen: {maxVaccineCnt}");
        
        // 우하향 대각선 체크
        for (int x = 0; x < _squareLength; x++)
        {
            for (int i = 0; i < _squareLength - x; i++)
            {
                switch (_squareControllerDict[x+i][i].State)
                {
                    case SquareController.SquareStateType.ZombieOccupied:
                        if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
                        currVaccineCnt = 0;
                        currZombieCnt++;
                        break;
                    
                    case SquareController.SquareStateType.VaccineOccupied:
                        if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
                        currZombieCnt = 0;
                        currVaccineCnt++;
                        break;
                    
                    default:
                        if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
                        if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
                        currVaccineCnt = 0;
                        currZombieCnt = 0;
                        break;
                }
            }
            if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
            if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
            currZombieCnt = 0;
            currVaccineCnt = 0;
        }
        
        for (int y = 0; y < _squareLength; y++)
        {
            for (int i = 0; i < _squareLength - y; i++)
            {
                switch (_squareControllerDict[i][y+i].State)
                {
                    case SquareController.SquareStateType.ZombieOccupied:
                        if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
                        currVaccineCnt = 0;
                        currZombieCnt++;
                        break;
                    
                    case SquareController.SquareStateType.VaccineOccupied:
                        if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
                        currZombieCnt = 0;
                        currVaccineCnt++;
                        break;
                    
                    default:
                        if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
                        if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
                        currVaccineCnt = 0;
                        currZombieCnt = 0;
                        break;
                }
            }
            if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
            if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
            currZombieCnt = 0;
            currVaccineCnt = 0;
        }
        
        // Debug.Log($"우하향 체크 - maxKing: {maxZombieCnt}, maxQueen: {maxVaccineCnt}");
        
        // 우상향 대각선 체크
        for (int x = 0; x < _squareLength; x++)
        {
            for (int i = 0; i < x+1; i++)
            {
                switch (_squareControllerDict[x-i][i].State)
                {
                    case SquareController.SquareStateType.ZombieOccupied:
                        if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
                        currVaccineCnt = 0;
                        currZombieCnt++;
                        break;
                    
                    case SquareController.SquareStateType.VaccineOccupied:
                        if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
                        currZombieCnt = 0;
                        currVaccineCnt++;
                        break;
                    
                    default:
                        if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
                        if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
                        currVaccineCnt = 0;
                        currZombieCnt = 0;
                        break;
                }
            }
            if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
            if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
            currZombieCnt = 0;
            currVaccineCnt = 0;
        }
        
        for (int y = 0; y < _squareLength; y++)
        {
            for (int i = 0; i < y+1; i++)
            {
                switch (_squareControllerDict[i][y-i].State)
                {
                    case SquareController.SquareStateType.ZombieOccupied:
                        if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
                        currVaccineCnt = 0;
                        currZombieCnt++;
                        break;
                    
                    case SquareController.SquareStateType.VaccineOccupied:
                        if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
                        currZombieCnt = 0;
                        currVaccineCnt++;
                        break;
                    
                    default:
                        if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
                        if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
                        currVaccineCnt = 0;
                        currZombieCnt = 0;
                        break;
                }
            }
            if (maxVaccineCnt < currVaccineCnt) maxVaccineCnt = currVaccineCnt;
            if (maxZombieCnt < currZombieCnt) maxZombieCnt = currZombieCnt;
            currZombieCnt = 0;
            currVaccineCnt = 0;
        }
        
        Debug.Log($"maxKing: {maxZombieCnt}, maxQueen: {maxVaccineCnt}");
        
        if (maxZombieCnt >= winCondition && maxVaccineCnt >= winCondition) _currState = GameStateType.Draw; 
        else if (maxZombieCnt >= winCondition) _currState = GameStateType.ZombieWin;
        else if (maxVaccineCnt >= winCondition) _currState = GameStateType.VaccineWin;
        else return false;
        
        EndGame();
        return true;
    }

    private void EndGame()
    {
        switch (_currState)
        {
            case GameStateType.ZombieWin:
                endingText.text = "좀비 승리!";
                AudioManager.PlaySfx(7);
                break;
            
            case GameStateType.VaccineWin:
                endingText.text = "인간 승리!";
                AudioManager.PlaySfx(7);
                break;
            
            case GameStateType.Draw:
                endingText.text = "무승부";
                AudioManager.PlaySfx(11);
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
                if(squareController.IsReadyToReserve)
                {
                    if (_currTurn == PlayerType.Zombie) zombieIconController.AddIcon();
                    else if (_currTurn == PlayerType.Vaccine) vaccineIconController.AddIcon();
                    _readyToReserveCnt--;
                }
                else if (_readyToReserveCnt >= reservedSquareCnt) return;
                else
                {
                    if (_currTurn == PlayerType.Zombie) zombieIconController.RemoveIcon();
                    else if (_currTurn == PlayerType.Vaccine) vaccineIconController.RemoveIcon();
                    _readyToReserveCnt++;
                }
                
                squareController.ToggleReadyToReserve(_currTurn == PlayerType.Zombie ? 0 : 1);
                AudioManager.PlaySfx(0);
                break;
            
            case GameStateType.Playing:
                if (isMoving) return;
                isMoving = true;
                squareController.Open(_currTurn == PlayerType.Zombie ? 0 : 1);
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
        _pushBtnBehavioursDict[pushDirection][id].MakeDisabled(disablePushBtnTurn);
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
                _pushBtnBehavioursDict[1][id].MakeDisabled(disablePushBtnTurn);
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
                _pushBtnBehavioursDict[0][id].MakeDisabled(disablePushBtnTurn);
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
                _pushBtnBehavioursDict[3][id].MakeDisabled(disablePushBtnTurn);
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
                _pushBtnBehavioursDict[2][id].MakeDisabled(disablePushBtnTurn);
                break;
        }
        
        AudioManager.PlaySfx(10);
    }

    public void PushEnded()
    {
        isMoving = false;
        RunGame();
    }
}
