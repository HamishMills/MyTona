using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public int bricksDestroyed;

    public int brickGoal;

    public Text score;

    // Start is called before the first frame update
    void Start()
    {
        bricksDestroyed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //score.text = (brickGoal - bricksDestroyed).ToString();
    }
}
