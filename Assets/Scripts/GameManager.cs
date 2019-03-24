using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Prefab brick
    public GameObject Brick;
    private GameObject currentBrick;

    private GameObject lastBrickClicked;

    private bool isDragging;

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
        //Debug.Log(checkValidity(new Vector2(0, 0)));
        checkValidity(new Vector2(0, 6));
    }

    // Update is called once per frame
    void Update()
    {
        if (checkDistance())
        {
            moveBlock((int)moveDirection(Direction));

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    
                }
            }
            // check validity.
            // if not return piece back.
            // if yes then destroy block, add points and everything fall to the ground.
        }
    }

    void newBoard()
    {
        for (int y = 0; y < 8; ++y)
        {
            for (int x = 0; x < 8; ++x)
            {
                Position = new Vector3(brickPositionX + (xIncrement * x), brickPositionY + (yIncrement * y));

                // Instantiate a brick at incrementing positions and set colour.
                currentBrick = Instantiate(Brick, Position, Quaternion.identity);
                currentBrick.GetComponent<BrickManager>().setBrickType((Random.Range(1, 6)));

                brickMap[x, y] = currentBrick;
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
                    //Debug.Log(tempVector);
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

    // maybe tempBrick only needs to copy position not actual object. setting position not actaul position in the array
    void moveBlock(int targetDirection)
    {
        Vector2 brickCoords = getClickedCoords(lastBrickClicked);
        Vector2 tempPosition = new Vector2(0,0);

        GameObject tempBrick;

        lastBrickClicked = null;

        if (targetDirection == 0)
        {
            // up
            if (brickCoords.y >= 7)
            {
                // do nothing
                Debug.Log("up");
            }
            else
            {
                tempPosition = brickMap[(int)brickCoords.x, (int)brickCoords.y + 1].transform.position;
                brickMap[(int)brickCoords.x, (int)brickCoords.y + 1].gameObject.transform.position = brickMap[(int)brickCoords.x, (int)brickCoords.y].transform.position;
                brickMap[(int)brickCoords.x, (int)brickCoords.y].gameObject.transform.position = new Vector2(tempPosition.x, tempPosition.y);

                tempBrick = brickMap[(int)brickCoords.x, (int)brickCoords.y];
                brickMap[(int)brickCoords.x, (int)brickCoords.y] = brickMap[(int)brickCoords.x, (int)brickCoords.y + 1];
                brickMap[(int)brickCoords.x, (int)brickCoords.y + 1] = tempBrick;
            }
        }
        if (targetDirection == 1)
        {
            // down
            if (brickCoords.y <= 0)
            {
                // do nothing
                Debug.Log("down");
            }
            else
            {
                tempPosition = brickMap[(int)brickCoords.x, (int)brickCoords.y - 1].transform.position;
                brickMap[(int)brickCoords.x, (int)brickCoords.y - 1].gameObject.transform.position = brickMap[(int)brickCoords.x, (int)brickCoords.y].transform.position;
                brickMap[(int)brickCoords.x, (int)brickCoords.y].gameObject.transform.position = new Vector2(tempPosition.x, tempPosition.y);

                tempBrick = brickMap[(int)brickCoords.x, (int)brickCoords.y];
                brickMap[(int)brickCoords.x, (int)brickCoords.y] = brickMap[(int)brickCoords.x, (int)brickCoords.y - 1];
                brickMap[(int)brickCoords.x, (int)brickCoords.y - 1] = tempBrick;
            }
        }
        else if (targetDirection == 2)
        {
            // left
            if (brickCoords.x <= 0)
            {
                // do nothing
                Debug.Log("left");
            }
            else
            {
                tempPosition = brickMap[(int)brickCoords.x - 1, (int)brickCoords.y].transform.position;
                brickMap[(int)brickCoords.x - 1, (int)brickCoords.y].gameObject.transform.position = brickMap[(int)brickCoords.x, (int)brickCoords.y].transform.position;
                brickMap[(int)brickCoords.x, (int)brickCoords.y].gameObject.transform.position = new Vector2(tempPosition.x, tempPosition.y);

                tempBrick = brickMap[(int)brickCoords.x, (int)brickCoords.y];
                brickMap[(int)brickCoords.x, (int)brickCoords.y] = brickMap[(int)brickCoords.x - 1, (int)brickCoords.y];
                brickMap[(int)brickCoords.x - 1, (int)brickCoords.y] = tempBrick;
            }
        }
        // targetDirection == 3
        else if (targetDirection == 3)
        {
            // right
            if (brickCoords.x >= 7)
            {
                // do nothing
                Debug.Log("right");
            }
            else
            {
                tempPosition = brickMap[(int)brickCoords.x + 1, (int)brickCoords.y].transform.position;
                brickMap[(int)brickCoords.x + 1, (int)brickCoords.y].gameObject.transform.position = brickMap[(int)brickCoords.x, (int)brickCoords.y].transform.position;
                brickMap[(int)brickCoords.x, (int)brickCoords.y].gameObject.transform.position = new Vector2(tempPosition.x, tempPosition.y);

                tempBrick = brickMap[(int)brickCoords.x, (int)brickCoords.y];
                brickMap[(int)brickCoords.x, (int)brickCoords.y] = brickMap[(int)brickCoords.x + 1, (int)brickCoords.y];
                brickMap[(int)brickCoords.x + 1, (int)brickCoords.y] = tempBrick;
            }
        }
        else
        {
            // do nothing
        }
    }

    public void setIsDragging(bool dragging)
    {
        isDragging = dragging;
    }

    public bool getIsDragging()
    {
        return isDragging;
    }

    public void setClickedObject(GameObject clickedObject)
    {
        lastBrickClicked = clickedObject;
    }

    // Check the distance between the center of the clicked block and the mouse cursor is over 0.5 (half of the size of a brick)
    public bool checkDistance()
    {
        //Debug.Log(lastBrickClicked);
        if (lastBrickClicked != null && isDragging == true)
        {
            worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));

            cursorDistance = getDistance(lastBrickClicked.transform.position.x, lastBrickClicked.transform.position.y, worldMousePosition.x, worldMousePosition.y);
            
            if (cursorDistance > 0.5)
            {
                Direction = worldMousePosition - lastBrickClicked.transform.position;
                Direction.z = 0;
                Direction.Normalize();

                return true;
            }
            else
            {
                return false;
            }
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

    // checks whether a single brick tile is currently in a row of 3 or more.
    Vector2 checkValidity(Vector2 brickCoord)
    {
        int brickType = brickMap[(int)brickCoord.x, (int)brickCoord.y].GetComponent<BrickManager>().getBrickType();

        int tempX = (int)brickCoord.x;
        int tempY = (int)brickCoord.y;

        // they start at -1 because the function counts itself twice. Once counting up, once counting down.
        int xBricks = -1;
        int yBricks = -1;

        // Vertical
        for (int y = (int)brickCoord.y; y < 8; y++)
        {
            if (brickMap[tempX, y].GetComponent<BrickManager>().getBrickType() == brickType)
            {
               // Debug.Log(yBricks);
                yBricks = yBricks + 1;
            }
            else
            {
                break;
            }
        }

        for (int y = (int)brickCoord.y; y >= 0; y--)
        {
            if (brickMap[tempX, y].GetComponent<BrickManager>().getBrickType() == brickType)
            {
                yBricks = yBricks + 1;
            }
            else
            {
                break;
            }
        }

        // Horizontal
        for (int x = (int)brickCoord.x; x < 8; x++)
        {
            if (brickMap[x, tempY].GetComponent<BrickManager>().getBrickType() == brickType)
            {
                xBricks = xBricks + 1;
            }
            else
            {
                break;
            }
        }

        for (int x = (int)brickCoord.x; x >= 0; x--)
        {
            if (brickMap[x, tempY].GetComponent<BrickManager>().getBrickType() == brickType)
            {
                xBricks = xBricks + 1;
            }
            else
            {
                break;
            }
        }
        return new Vector2(xBricks, yBricks);
    }
}







// might have to do y+1
//while (brickMap[currentX, currentY] != null)
//{
//    if (plusValues)
//    {
//        if (brickMap[currentX, currentY + 1] == null)
//        {
//            currentY = (int)brickCoord.y;
//            plusValues = false;
//        }
//        else
//        {
//            currentY += 1;
//        }
//    }
//    else
//    {
//        currentY -= 1;
//        if (brickMap[currentX, currentY - 1] == null)
//        {
//            currentY = (int)brickCoord.y;
//            plusValues = true;
//        }
//        else
//        {
//            currentY -= 1;

//        }
//    }
//}