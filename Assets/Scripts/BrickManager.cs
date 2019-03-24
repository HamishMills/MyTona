using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickManager : MonoBehaviour
{
    [HideInInspector]
    public int brickType;

    private GameManager gameManagerScript;

    public Vector3 offsetPosition;

    // Start is called before the first frame update
    void Start()
    {
        gameManagerScript = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int getBrickType()
    {
        return brickType;
    }


    public void setBrickType(int newBrickType)
    {
        brickType = newBrickType;

        if (brickType == 1)
        {
            this.GetComponent<Renderer>().material.color = Color.red;
        }
        else if (brickType == 2)
        {
            this.GetComponent<Renderer>().material.color = Color.blue;
        }
        else if (brickType == 3)
        {
            this.GetComponent<Renderer>().material.color = Color.green;
        }
        else if (brickType == 4)
        {
            this.GetComponent<Renderer>().material.color = Color.magenta;
        }
        else
        {
            brickType = 5;
            this.GetComponent<Renderer>().material.color = Color.yellow;
        }
    }

    public void deleteBrick()
    {
        Destroy(this.gameObject);
    }

    void OnMouseDown()
    {
        gameManagerScript.setIsDragging(true);
        gameManagerScript.setClickedObject(this.gameObject);
      
        offsetPosition = gameObject.transform.position -
        Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
    }

    void OnMouseUp()
    {
        gameManagerScript.setIsDragging(false);
    }

    //void OnMouseDrag()
    //{
    //    if (!gameManagerScript.getIsDragging())
    //    {
    //        gameManagerScript.checkDistance();
    //        Debug.Log("Test");
    //    }
    //}
}