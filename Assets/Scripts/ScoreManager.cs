using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public int bricksDestroyed;
    public int percentIncrease = 15;
    public float brickGoal;

    public Text score;

    private GameManager gameManagerScript;
    private ScoreScript Remaining;
    private LevelSet level;

    // Start is called before the first frame update
    void Start()
    {
        bricksDestroyed = 0;
        brickGoal = 150;
        gameManagerScript = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        Remaining = GameObject.FindGameObjectWithTag("TimeRemaining").GetComponent<ScoreScript>();
        level = GameObject.FindGameObjectWithTag("level").GetComponent<LevelSet>();
    }

    // Update is called once per frame
    void Update()
    {
        if (brickGoal - bricksDestroyed <= 0)
        {
            level.levelIncrement();
            gameManagerScript.resetBoard();
            Remaining.resetTimeRemaining();
            bricksDestroyed = 0;
            brickGoal += (int)(brickGoal / 100) * percentIncrease;
        }
        score.text = (brickGoal - bricksDestroyed).ToString();
    }

    public void setBricksDestroyed()
    {
        bricksDestroyed += 1;
    }
}
