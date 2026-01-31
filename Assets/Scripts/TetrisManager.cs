using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TetrisManager : MonoBehaviour
{
    public int score { get; private set; }

        public bool gameOver { get; private set; }

    public UnityEvent OnScoreChanged;
    public UnityEvent OnGameOver;
    
    // Start is called before the first frame update
    void Start()
    {
        SetGameOver(false);
    }

    public int CalculateScore(int clearedRows)
    {
        switch (clearedRows)
        {
            case 1: return 100;
            case 2: return 300;
            case 3: return 400;
            case 4: return 800;
            default: return 0;
        }
    }

    public void ChangeScore(int amount)
    {
        score += amount;
        OnScoreChanged.Invoke();
    }

    public void SetGameOver(bool gameOver)
    {
        if (!gameOver)
        {
            score = 0;
            ChangeScore(0);
        }

        this.gameOver = gameOver;
        OnGameOver.Invoke();

    }

}