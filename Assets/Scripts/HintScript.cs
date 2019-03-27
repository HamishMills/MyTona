using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintScript : MonoBehaviour
{
    private GameManager gameManagerScript;

    // Start is called before the first frame update
    void Start()
    {
        gameManagerScript = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(645, 550, 450, 50), "Hint"))
        {
            Debug.Log(gameManagerScript.checkPossibleMove());
        }
        if (GUI.Button(new Rect(645, 475, 450, 50), "Shuffle"))
        {
            gameManagerScript.resetBoard();
        }
        //Debug.Log("Clicked the button with an image");
    }
}
