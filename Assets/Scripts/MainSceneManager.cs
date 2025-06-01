using System;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneManager : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Transform[] columns;
    
    [Header("Values")] 
    [SerializeField] private int reservedSquareCnt;
    [SerializeField] private Sprite[] numberSprites;

    private Dictionary<int, Dictionary<int, SquareController>> _squareControllerDict;

    private enum PlayerType
    {
        King,
        Queen,
    }

    private PlayerType _currTurn;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        _currTurn = PlayerType.King;
        
        // 각각의 square 초기화
        _squareControllerDict = new Dictionary<int, Dictionary<int, SquareController>>();
        
        for (int x = 0; x < columns.Length; x++)
        {
            _squareControllerDict.Add(x, new Dictionary<int, SquareController>());
            
            for (int y = 0; y < columns.Length; y++)
            {
                int id = x * columns.Length + y;
                
                SquareController controller = columns[x].GetChild(y).gameObject.GetComponent<SquareController>();
                controller.Init(id, numberSprites[id]);
                _squareControllerDict[x].Add(y, controller);
            }
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

    private void CheckWinner()
    {
        
    }
}
