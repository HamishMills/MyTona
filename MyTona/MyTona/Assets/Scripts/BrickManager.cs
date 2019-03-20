using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickManager : MonoBehaviour
{
    [HideInInspector]
    public int brickType;

    private float cursorDistance;

    private Vector3 offsetPosition;
    private Vector3 worldMousePosition;
    private Vector3 Direction;

    enum targetDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public int GetBrickType()
    {
        return brickType;
    }

    public void SetBrickType(int newBrickType)
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

    void OnMouseDown()
    {
        offsetPosition = gameObject.transform.position -
        Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
    }

    void OnMouseDrag()
    {
        worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));

        cursorDistance = getDistance(this.transform.position.x, this.transform.position.y, worldMousePosition.x, worldMousePosition.y);

        //Debug.Log(getDistance(this.transform.position.x, this.transform.position.y, worldMousePosition.x, worldMousePosition.y));

        if (cursorDistance > 0.5)
        {
            Direction = worldMousePosition - this.transform.position;
            Direction.z = 0;
            Direction.Normalize();

        }

        Debug.Log(wheredoigo(Direction));
        // if x > 0 then right, if y > 0 up down


        //Vector3 newPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        //transform.position = Camera.main.ScreenToWorldPoint(newPosition) + offsetPosition;
    }

    float getDistance(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt(Mathf.Pow((x2 - x1), 2) + Mathf.Pow((y2 - y1), 2));
    }

    targetDirection wheredoigo(Vector2 direction)
    {

        // outside of function do allocation once off.
        // allocations are horizontal and vert if statmeents.
        // possible outputs = horiztontal or vertical.

        Vector2 absVec = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
        float BiggerComponent;

        targetDirection[] PossibleOutputs;

        if (absVec.x > absVec.y)
        {
            BiggerComponent = direction.x;

            // horizontal
            // x
            PossibleOutputs = new targetDirection[2] { targetDirection.Left, targetDirection.Right };
        }
        else
        {
            BiggerComponent = direction.y;
            // Vert or diagonal
            //y 
            PossibleOutputs = new targetDirection[2] { targetDirection.Down, targetDirection.Up };

        }

        if (BiggerComponent > 0)
        {
            return PossibleOutputs[1];
        }
        else
        {
            return PossibleOutputs[0];
        }
    }

    // maybe the delete function should be in the game manager
    public void deleteBrick()
    {
        // remove from the array
        // this = script and not the game object
        //Destroy(this.gameObject);
    }
}
