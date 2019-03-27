using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintScript : MonoBehaviour
{
    private GameManager gameManagerScript;
    private CoordinateTextScript coordsX;
    private CoordinateTextScript coordsY;

    public GameObject background;

    public int buttonType;
    // Start is called before the first frame update
    void Start()
    {
        gameManagerScript = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        coordsY = GameObject.FindGameObjectWithTag("coords").GetComponent<CoordinateTextScript>();
        coordsX = GameObject.FindGameObjectWithTag("xcoords").GetComponent<CoordinateTextScript>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        if (buttonType == 1)
        {
            coordsX.setCoordinates();
            coordsY.setCoordinates();
           // Debug.Log(gameManagerScript.checkPossibleMove());
        }
        if (buttonType == 2)
        {
            gameManagerScript.resetBoard();
        }
        if (buttonType == 3)
        {
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0;
                background.GetComponent<SpriteRenderer>().enabled = false;
            }
            else
            {
                Time.timeScale = 1;
                background.GetComponent<SpriteRenderer>().enabled = true;
            }
        }
        if (buttonType == 4)
        {
            Application.Quit();
        }
    }
}
