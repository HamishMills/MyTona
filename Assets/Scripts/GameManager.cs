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

    // Gets two different positions of destroyable rows either going vertical or horizontal.
    private Vector2 deleteCoordsY = new Vector2(0, 0);
    private Vector2 deleteCoordsX = new Vector2(0, 0);
    private Vector2 lastBrickCoords = new Vector2(0, 0);

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

    // Distance each tile needs to move. 
    // So we can * the increment number by column/row to get distance.
    [SerializeField]
    private float xIncrement = 1.5f;
    [SerializeField]
    private float yIncrement = 1.5f;

    Vector3 Position;

    void Start()
    {
        // Creates a board of random colours.
        newBoard();

        // Changes the board so that there aren't any possible solves to begin with.
        fixNewBoard();
    }

    // Update is called once per frame
    void Update()
    {
        // Constantly look for potential solves
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (brickMap[x,y] != null)
                {
                    destroySolvedBricks(checkValidity(new Vector2(x, y)));
                }
            }
        }

        if (checkDistance())
        {
            moveBrick((int)moveDirection(Direction));
            // check validity.
            // if not return piece back.
            // if yes then destroy brick, add points and everything fall to the ground.
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

    // Simple math formula for getting distance, could also use vectors.
    float getDistance(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt(Mathf.Pow((x2 - x1), 2) + Mathf.Pow((y2 - y1), 2));
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

                currentBrick.GetComponent<BrickManager>().setBrickType(Random.Range(1, 6));
                //currentBrick.GetComponent<BrickManager>().setBrickType(1);

                brickMap[x, y] = currentBrick;
            }
        }
        futureBrickMap = brickMap;
    }

    void fixNewBoard()
    {
        // for all objects within brickMap[x,y]
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                // check if every object has more than 3 blocks of the same colour in a row.
                if (checkValidity(new Vector2(x, y)).x >= 3 || checkValidity(new Vector2(x, y)).y >= 3)
                {
                    // if yes then keep setting the brickMap[x,y] to a different colour until brickMap[x,y] is less than 3.
                    for (int i = 1; i <= 5; i++)
                    {
                        brickMap[x, y].GetComponent<BrickManager>().setBrickType(i);
                        if (checkValidity(new Vector2(x, y)).x >= 3 || checkValidity(new Vector2(x, y)).y >= 3)
                        {
                            // do nothing. It is the same coloured brick as it was before.
                        }
                        else
                        {
                            // exit from the colour for loop and go onto the next block.
                            break;
                        }
                    }
                }
            }
        }
    }

    Vector2 getClickedCoords(GameObject clickedObject)
    {
        Vector2 tempVector = new Vector2(0,0);

        // For each object in the brickMap[,] array.
        // Check if the last clicked object is that object.
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (brickMap[x,y] == clickedObject)
                {
                    tempVector.x = x;
                    tempVector.y = y;

                    return tempVector;
                }
            }
        }
        Debug.Log("getClickedObject() returned NULL");
        return tempVector;
    }

    // Gets the direction the mouse is in relation to the last clicked block. 
    // Then if legal, moves the block in that direction and swaps it with the block in the other direction.
    void moveBrick(int targetDirection)
    {
        lastBrickCoords = getClickedCoords(lastBrickClicked);
        Vector2 tempPosition = new Vector2(0,0);

        GameObject tempBrick;

        lastBrickClicked = null;

        // 0 == up
        if (targetDirection == 0)
        {
            if (lastBrickCoords.y >= 7)
            {
                // do nothing
            }
            else
            {
                // Swap the two physical positions of each brick.
                // Later on should do this over time and not teleport.
                tempPosition = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y + 1].transform.position;
                brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y + 1].gameObject.transform.position = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y].transform.position;
                brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y].gameObject.transform.position = new Vector2(tempPosition.x, tempPosition.y);

                // Swap the two positions within the brickMap[,] array.
                tempBrick = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y];
                brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y] = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y + 1];
                brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y + 1] = tempBrick;
            }
        }
        // 1 == down
        if (targetDirection == 1)
        {
            if (lastBrickCoords.y <= 0)
            {
                // do nothing
            }
            else
            {
                // Swap the two physical positions of each brick.
                // Later on should do this over time and not teleport.
                tempPosition = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y - 1].transform.position;
                brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y - 1].gameObject.transform.position = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y].transform.position;
                brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y].gameObject.transform.position = new Vector2(tempPosition.x, tempPosition.y);

                // Swap the two positions within the brickMap[,] array.
                tempBrick = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y];
                brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y] = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y - 1];
                brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y - 1] = tempBrick;
            }
        }
        // 2 == left
        else if (targetDirection == 2)
        {
            if (lastBrickCoords.x <= 0)
            {
                // do nothing
            }
            else
            {
                // Swap the two physical positions of each brick.
                // Later on should do this over time and not teleport.
                tempPosition = brickMap[(int)lastBrickCoords.x - 1, (int)lastBrickCoords.y].transform.position;
                brickMap[(int)lastBrickCoords.x - 1, (int)lastBrickCoords.y].gameObject.transform.position = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y].transform.position;
                brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y].gameObject.transform.position = new Vector2(tempPosition.x, tempPosition.y);

                // Swap the two positions within the brickMap[,] array.
                tempBrick = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y];
                brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y] = brickMap[(int)lastBrickCoords.x - 1, (int)lastBrickCoords.y];
                brickMap[(int)lastBrickCoords.x - 1, (int)lastBrickCoords.y] = tempBrick;
            }
        }
        // 3 == right
        else if (targetDirection == 3)
        {
            if (lastBrickCoords.x >= 7)
            {
                // do nothing
            }
            else
            {
                // Swap the two physical positions of each brick.
                // Later on should do this over time and not teleport.
                tempPosition = brickMap[(int)lastBrickCoords.x + 1, (int)lastBrickCoords.y].transform.position;
                brickMap[(int)lastBrickCoords.x + 1, (int)lastBrickCoords.y].gameObject.transform.position = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y].transform.position;
                brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y].gameObject.transform.position = new Vector2(tempPosition.x, tempPosition.y);

                // Swap the two positions within the brickMap[,] array.
                tempBrick = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y];
                brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y] = brickMap[(int)lastBrickCoords.x + 1, (int)lastBrickCoords.y];
                brickMap[(int)lastBrickCoords.x + 1, (int)lastBrickCoords.y] = tempBrick;
            }
        }
        else
        {
            // do nothing
        }
    }

    // Check the distance between the center of the clicked brick and the mouse cursor is over 0.5 (half of the size of a brick)
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

    // Gets the direction the mouse is pointing in in relation to the brick.
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
        for (int y = (int)brickCoord.y; y >= 0; y--)
        {
            if (brickMap[tempX, y] != null && brickMap[tempX, y].GetComponent<BrickManager>().getBrickType() == brickType)
            {
                yBricks = yBricks + 1;

                // Set the Y coords to be the block position itself.
                // If the highest Y position is higher than the block position, it'll change in the next for loop.
                deleteCoordsY = new Vector2(tempX, tempY);
            }
            else
            {
                break;
            }
        }

        for (int y = (int)brickCoord.y; y < 8; y++)
        {
            if (brickMap[tempX, y] != null && brickMap[tempX, y].GetComponent<BrickManager>().getBrickType() == brickType)
            {
                yBricks = yBricks + 1;

                // We want the highest y value of a vertical row so that we can easily find the right blocks
                // For (brickY) number of objects down from the highest Y value, delete/fall/replace.
                deleteCoordsY = new Vector2(tempX, y);
            }
            else
            {
                break;
            }
        }

        // Horizontal

        for (int x = (int)brickCoord.x; x >= 0; x--)
        {
            if (brickMap[x, tempY] != null && brickMap[x, tempY].GetComponent<BrickManager>().getBrickType() == brickType)
            {
                xBricks = xBricks + 1;

                // Set the X coords to be the block position itself.
                // If the highest X position is higher than the block position, it'll change in the next for loop.
                deleteCoordsX = new Vector2(tempX, tempY);
            }
            else
            {
                break;
            }
        }
        for (int x = (int)brickCoord.x; x < 8; x++)
        {
            if (brickMap[x, tempY] != null && brickMap[x, tempY].GetComponent<BrickManager>().getBrickType() == brickType)
            {
                xBricks = xBricks + 1;

                // We want the highest x value of a vertical row so that we can easily find the right blocks
                // For (brickY) number of objects down from the highest X value, delete/fall/replace.
                deleteCoordsX = new Vector2(x, tempY);
            }
            else
            {
                break;
            }
        }

        return new Vector2(xBricks, yBricks);
    }

    void destroySolvedBricks(Vector2 brickCount)
    {
        Vector2 currentValidityNumber;
        Vector2 newValidityNumber = new Vector2(0, 0);

        if (brickCount.x >= 3 || brickCount.y >= 3)
        {
            if (brickCount.x > brickCount.y)
            {
                currentValidityNumber = checkValidity(new Vector2(deleteCoordsX.x, deleteCoordsX.y));

                for (int i = (int)deleteCoordsX.x; i > (int)deleteCoordsX.x - brickCount.x; i--)
                {
                    if ((int)currentValidityNumber.x + (int)currentValidityNumber.y >= (int)checkValidity(new Vector2(i, deleteCoordsX.y)).x + (int)checkValidity(new Vector2(i, deleteCoordsX.y)).y)
                    {
                        // do nothing. biggest value is most valuable.
                    }
                    else
                    {
                        newValidityNumber = new Vector2(i, deleteCoordsX.y);
                        currentValidityNumber = checkValidity(new Vector2(i, deleteCoordsX.y));
                    }
                }

               // brickCount = checkValidity(newValidityNumber);
            }
            else if (brickCount.y > brickCount.x)
            {
                currentValidityNumber = checkValidity(new Vector2(deleteCoordsY.x, deleteCoordsY.y));

                for (int i = (int)deleteCoordsY.y; i > (int)deleteCoordsY.y - brickCount.y; i--)
                {
                    if ((int)currentValidityNumber.x + (int)currentValidityNumber.y >= (int)checkValidity(new Vector2(deleteCoordsY.x, i)).x + (int)checkValidity(new Vector2(deleteCoordsY.x, i)).y)
                    {
                        // do nothing. biggest value is most valuable.
                    }
                    else
                    {
                        newValidityNumber = new Vector2(deleteCoordsY.x, i);
                        currentValidityNumber = checkValidity(new Vector2(deleteCoordsY.x, i));
                    }
                }
            }
            else 
            {
                // both are the same so the brick must be the best position
            }
        }
            // double check that it is a possible move.
        if (brickCount.y >= 3)
        {
            for (int i = (int)deleteCoordsY.y; i > ((int)deleteCoordsY.y - brickCount.y); i--)
            {
                brickMap[(int)deleteCoordsY.x, i].GetComponent<BrickManager>().deleteBrick();
                brickMap[(int)deleteCoordsY.x, i] = null;
            }
        }
                
        if (brickCount.x >= 3)
        {
            for (int i = (int)deleteCoordsX.x; i > ((int)deleteCoordsX.x - brickCount.x); i--)
            {
                if (brickMap[i, (int)deleteCoordsX.y] != null)
                {
                    brickMap[i, (int)deleteCoordsX.y].GetComponent<BrickManager>().deleteBrick();
                    brickMap[i, (int)deleteCoordsX.y] = null;
                }
            }
        }
    }
}