using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    public float timeRemaining;

    public Text time;

    // Start is called before the first frame update
    void Start()
    {
        timeRemaining = 180.0f;
    }

    // Update is called once per frame
    void Update()
    {
        timeRemaining -= Time.deltaTime;
        time.text = timeRemaining.ToString("0");

        if (timeRemaining <= 0)
        {
            Application.Quit();
        }
    }

    public void resetTimeRemaining()
    {
        timeRemaining = 180.0f;
    }
}
