using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    public TetrisManager tetrisManager;

    public GameObject endGamePanel;

    public void UpdateScore()
    {
        scoreText.text = $"SCORE: {tetrisManager.score:n0}";
    }

    public void UpdateGameOver()
    {
        //When the game over Event is broadcasted, the end game panel is showed, then hides when the game resets
        endGamePanel.SetActive(tetrisManager.gameOver);
    }

    public void PlayAgain()
    {
        //Setting game over to false resets the game
        tetrisManager.SetGameOver(false);
    }
}
