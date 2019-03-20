using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Prefab brick
    public GameObject Brick;
    private GameObject currentBrick;

    private GameObject lastBrickClicked;

    // Position of the bottom left most tile.
    private float brickPositionX = -9.0f;
    private float brickPositionY = -4.0f;

    // Distance between cursor and selected brick.
    private float cursorDistance;

    private Vector3 worldMousePosition;
    private Vector3 Direction;

    private GameObject[,] brickMap = new GameObject[8, 8];
    private GameObject[,] futureBrickMap = new GameObject[8, 8];

    private int[,] colourMap = new int[8, 8];

    public enum targetDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    //targetDirection[] PossibleOutputs = new targetDirection[2];

    // Distance each tile needs to move. 
    // So we can * the increment number by column/row to get distance.

    [SerializeField]
    private float xIncrement = 1.5f;

    [SerializeField]
    private float yIncrement = 1.5f;

    Vector3 Position;

    void Start()
    {
        newBoard();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(checkDirection());
        if (checkDirection())
        {
            //moveBlock((int)moveDirection(Direction));
            //Debug.Log((int)moveDirection(Direction));
            
        }

    }

    void newBoard()
    {
        for (int y = 0; y < 8; ++y)
        {
            for (int x = 0; x < 8; ++x)
            {
                //colourMap[x, y] = Random.Range(1, 6);

                Position = new Vector3(brickPositionX + (xIncrement * x), brickPositionY + (yIncrement * y));

                // Instantiate a brick at incrementing positions and set colour.
                currentBrick = Instantiate(Brick, Position, Quaternion.identity);
                currentBrick.GetComponent<BrickManager>().SetBrickType((Random.Range(1, 6)));

                brickMap[x, y] = currentBrick;
                // brickMap[x, y] = currentBrick.GetComponent<BrickManager>().GetBrickType();
            }
        }

        futureBrickMap = brickMap;
    }

    Vector2 getClickedCoords(GameObject clickedObject)
    {
        Vector2 tempVector = new Vector2(0,0);

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (brickMap[x,y] == clickedObject)
                {
                    tempVector.x = x;
                    tempVector.y = y;
                    Debug.Log(tempVector);
                    return tempVector;
                }
            }
        }
        Debug.Log("getClickedObject() returned (0,0)");
        return tempVector;
    }

    float getDistance(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt(Mathf.Pow((x2 - x1), 2) + Mathf.Pow((y2 - y1), 2));
    }

    // maybe tempBrick only needs to copy position not actual object.
    void moveBlock(int targetDirection)
    {
        Vector2 brickCoords = getClickedCoords(lastBrickClicked);
        GameObject tempBrick;

        if (targetDirection == 0)
        {
            // up
            if (brickCoords.y >= 7)
            {
                // do nothing
            }
            else
            {
                tempBrick = brickMap[(int)brickCoords.x,(int)brickCoords.y + 1];

                brickMap[(int)brickCoords.x, (int)brickCoords.y + 1].gameObject.transform.position
                    .Set(brickCoords.x,brickCoords.y,0);

                brickMap[(int)brickCoords.x, (int)brickCoords.y].gameObject.transform.position
                   .Set(tempBrick.transform.position.x, tempBrick.transform.position.y, 0);
            }
        }
        if (targetDirection == 1)
        {
            // down
            if (brickCoords.y <= 0)
            {
                // do nothing
            }
            else
            {
                tempBrick = brickMap[(int)brickCoords.x, (int)brickCoords.y - 1];

                brickMap[(int)brickCoords.x, (int)brickCoords.y - 1].gameObject.transform.position
                    .Set(brickCoords.x, brickCoords.y, 0);

                brickMap[(int)brickCoords.x, (int)brickCoords.y].gameObject.transform.position
                   .Set(tempBrick.transform.position.x, tempBrick.transform.position.y, 0);
            }
        }
        if (targetDirection == 2)
        {
            // left
            if (brickCoords.x <= 0)
            {
                // do nothing
            }
            else
            {
                tempBrick = brickMap[(int)brickCoords.x - 1, (int)brickCoords.y];

                brickMap[(int)brickCoords.x - 1, (int)brickCoords.y].gameObject.transform.position
                    .Set(brickCoords.x, brickCoords.y, 0);

                brickMap[(int)brickCoords.x, (int)brickCoords.y].gameObject.transform.position
                   .Set(tempBrick.transform.position.x, tempBrick.transform.position.y, 0);
            }
        }
        if (targetDirection == 3)
        {
            // right
            if (brickCoords.x >= 7)
            {
                // do nothing
            }
            else
            {
                tempBrick = brickMap[(int)brickCoords.x + 1, (int)brickCoords.y];

                brickMap[(int)brickCoords.x + 1, (int)brickCoords.y].gameObject.transform.position
                    .Set(brickCoords.x, brickCoords.y, 0);

                brickMap[(int)brickCoords.x, (int)brickCoords.y].gameObject.transform.position
                   .Set(tempBrick.transform.position.x, tempBrick.transform.position.y, 0);
            }
        }
    }

    public void getClickedObject(GameObject clickedObject)
    {
        lastBrickClicked = clickedObject;
    }

    public bool checkDirection()
    {
        if (lastBrickClicked == null)
        {
            return false;
        }

        worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));

        cursorDistance = getDistance(lastBrickClicked.transform.position.x, lastBrickClicked.transform.position.y, worldMousePosition.x, worldMousePosition.y);
        // You have to time it perfectly, if you click on the center. lastBrickClicked is not null but the next frame it will be.
        if (cursorDistance > 0.5)
        {
            Direction = worldMousePosition - lastBrickClicked.transform.position;
            Direction.z = 0;
            Direction.Normalize();
            lastBrickClicked = null;
            return true;
        }

        return false;
    }

    // Gets the direction the mouse is pointing in in relation to the block.
    public targetDirection moveDirection(Vector2 direction)
    {
        Vector2 absVec = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
        targetDirection[] PossibleOutputs;

        float biggerComponent;

        // Horizontal
        if (absVec.x > absVec.y)
        {
            biggerComponent = direction.x;

            PossibleOutputs = new targetDirection[2] { targetDirection.Left, targetDirection.Right };
        }

        // Vertical
        else
        {
            biggerComponent = direction.y;

            PossibleOutputs = new targetDirection[2] { targetDirection.Down, targetDirection.Up };
        }

        if (biggerComponent > 0)
        {
            return PossibleOutputs[1];
        }
        else
        {
            return PossibleOutputs[0];
        }
    }
}
