using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoordinateTextScript : MonoBehaviour
{
    private GameManager gameManagerScript;

    public Text xCoord;
    public Text yCoord;

    public int buttonType;

    public bool hintActivated;

    public float coordTimer = 5.0f;
    // Start is called before the first frame update
    void Start()
    {
        gameManagerScript = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        hideCoordinates();
    }

    // Update is called once per frame
    void Update()
    {
        if (hintActivated)
        {
            coordTimer -= Time.deltaTime;
        }

        if (coordTimer < 0)
        {
            hintActivated = false;
            coordTimer = 5.0f;
            hideCoordinates();
        }
       
    }

    public void setCoordinates()
    {
        hintActivated = true;
        if (buttonType == 1)
        {
            xCoord.text = (gameManagerScript.checkPossibleMove().x + 1).ToString();
        }
        if (buttonType == 2)
        {
            yCoord.text = (gameManagerScript.checkPossibleMove().y + 1).ToString();
        }
    }

    public void hideCoordinates()
    {
        xCoord.text = "";
        yCoord.text = "";
    }
}
