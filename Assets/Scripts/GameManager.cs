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
    private bool didMove;
    private bool hasDestroyed;

    public bool finishReverse = true;

    // Position of the bottom left most tile.
    private float brickPositionX = -10.8f;
    private float brickPositionY = -4.0f;

    private float reverseTimer = 0.5f;
    private float timeSinceLastMove = 30.0f;

    // Distance between cursor and selected brick.
    private float cursorDistance;

    // Distance each tile needs to move. 
    // So we can * the increment number by column/row to get distance.
    [SerializeField]
    private float xIncrement = 1f;
    [SerializeField]
    private float yIncrement = 1f;

    private Vector3 worldMousePosition;
    private Vector3 Direction;
    private Vector3 Position;

    // Gets two different positions of destroyable rows either going vertical or horizontal.
    private Vector2 deleteCoordsY = new Vector2(0, 0);
    private Vector2 deleteCoordsX = new Vector2(0, 0);

    private Vector2 tempDeleteCoordsY = new Vector2(0, 0);
    private Vector2 tempDeleteCoordsX = new Vector2(0, 0);

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
        // Bricks fall down to the lowest untaken position (gravity)
        setFall();

        // Fill the null positions with bricks.
        setRefill();

        // Constantly look for potential solves and destroy them.
        setDestroy();

        if (checkDistance() && finishReverse)
        {
            moveBrick((int)moveDirection(Direction));

            hasDestroyed = false;

            didMove = true;
        }

        // if nothing has been destroyed then reverse the move
        if (didMove && !hasDestroyed)
        {
            if (reverseTimer < 0)
            {
                reverseMove((int)moveDirection(Direction));
                reverseTimer = 0.5f;
                didMove = false;
                finishReverse = true;
            }
            else if (didMove && !hasDestroyed)
            {
                reverseTimer -= Time.deltaTime;
                finishReverse = false;
            }
        }
        else
        {
            // if something was destroyed
            finishReverse = true;
        }

        // if 30 seconds have passed without the player solving anything
        // tell them where a solvable position is.
        if (timeSinceLastMove < 0)
        {
            timeSinceLastMove = 30.0f;
            Debug.Log(checkPossibleMove());
        }
        else
        {
            timeSinceLastMove -= Time.deltaTime;
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

    public void resetBoard()
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                brickMap[x, y].GetComponent<BrickManager>().deleteBrick();
                brickMap[x, y] = null;
            }
        }
        newBoard();
        fixNewBoard();
    }

    void fixNewBoard()
    {
        // for all objects within brickMap[x,y]
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                // check if every object has more than 3 bricks of the same colour in a row.
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
                            // exit from the colour for loop and go onto the next brick.
                            break;
                        }
                    }
                }
            }
        }
        setColourMap();
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
        return tempVector;
    }

    // Gets the direction the mouse is in relation to the last clicked brick. 
    // Then if legal, moves the brick in that direction and swaps it with the brick in the other direction.
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
                didMove = false;
            }
            else
            {
                didMove = true;

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
                didMove = false;
            }
            else
            {
                didMove = true;

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
                didMove = false;
            }
            else
            {
                didMove = true;
                
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
                didMove = false;
            }
            else
            {
                didMove = true;

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

    // Reverse the last move made if the brick is not in a solved state.
    void reverseMove(int targetDirection)
    {
        Vector2 tempPosition = new Vector2(0, 0);
        GameObject tempBrick;

        //0 == up
        if (targetDirection == 0)
        {
            if (lastBrickCoords.y >= 7)
            {
                // do nothing
            }
            else
            {
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
        else if (targetDirection == 1)
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
                
            //reverseDirection = 0;
            //lastBrickClicked = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y - 1];
        }
        // 2 == left
        else if (targetDirection == 2)
        {
            if (lastBrickCoords.x <= 0)
            {
                // do nothing
                didMove = false;
            }
            else
            {
                tempPosition = brickMap[(int)lastBrickCoords.x - 1, (int)lastBrickCoords.y].transform.position;
                brickMap[(int)lastBrickCoords.x - 1, (int)lastBrickCoords.y].gameObject.transform.position = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y].transform.position;
                brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y].gameObject.transform.position = new Vector2(tempPosition.x, tempPosition.y);

                // Swap the two positions within the brickMap[,] array.
                tempBrick = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y];
                brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y] = brickMap[(int)lastBrickCoords.x - 1, (int)lastBrickCoords.y];
                brickMap[(int)lastBrickCoords.x - 1, (int)lastBrickCoords.y] = tempBrick;
            }
                
            //reverseDirection = 3;
            //lastBrickClicked = brickMap[(int)lastBrickCoords.x - 1, (int)lastBrickCoords.y];
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
                tempPosition = brickMap[(int)lastBrickCoords.x + 1, (int)lastBrickCoords.y].transform.position;
                brickMap[(int)lastBrickCoords.x + 1, (int)lastBrickCoords.y].gameObject.transform.position = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y].transform.position;
                brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y].gameObject.transform.position = new Vector2(tempPosition.x, tempPosition.y);

                // Swap the two positions within the brickMap[,] array.
                tempBrick = brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y];
                brickMap[(int)lastBrickCoords.x, (int)lastBrickCoords.y] = brickMap[(int)lastBrickCoords.x + 1, (int)lastBrickCoords.y];
                brickMap[(int)lastBrickCoords.x + 1, (int)lastBrickCoords.y] = tempBrick;
            }
            //reverseDirection = 2;
            //lastBrickClicked = brickMap[(int)lastBrickCoords.x + 1, (int)lastBrickCoords.y];

            // Swap the two physical positions of each brick.
            // Later on should do this over time and not teleport.
            
        }
        //moveBrick(reverseDirection);
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

        float biggerComponent = 0;

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

                // Set the Y coords to be the brick position itself.
                // If the highest Y position is higher than the brick position, it'll change in the next for loop.
                tempDeleteCoordsY = new Vector2(tempX, tempY);
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

                // We want the highest y value of a vertical row so that we can easily find the right bricks
                // For (brickY) number of objects down from the highest Y value, delete/fall/replace.
                tempDeleteCoordsY = new Vector2(tempX, y);
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

                // Set the X coords to be the brick position itself.
                // If the highest X position is higher than the brick position, it'll change in the next for loop.
                tempDeleteCoordsX = new Vector2(tempX, tempY);
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

                // We want the highest x value of a vertical row so that we can easily find the right bricks
                // For (brickY) number of objects down from the highest X value, delete/fall/replace.
                tempDeleteCoordsX = new Vector2(x, tempY);
            }
            else
            {
                break;
            }
        }
        return new Vector2(xBricks, yBricks);
    }

    // Checks the validity of future positions
    Vector2 checkColourValidity(Vector2 brickCoord)
    {
        //int brickType = futureBrickMap[(int)brickCoord.x, (int)brickCoord.y].GetComponent<BrickManager>().getBrickType();

        int tempX = (int)brickCoord.x;
        int tempY = (int)brickCoord.y;

        // they start at -1 because the function counts itself twice. Once counting up, once counting down.
        int xBricks = -1;
        int yBricks = -1;

        // Vertical
        for (int y = (int)brickCoord.y; y >= 0; y--)
        {
            if (colourMap[tempX, y] == colourMap[tempX, tempY])
            {
                yBricks = yBricks + 1;
            }
            else
            {
                break;
            }
        }

        for (int y = (int)brickCoord.y; y < 8; y++)
        {
            if (colourMap[tempX, y] == colourMap[tempX, tempY])
            {
                yBricks = yBricks + 1;
            }
            else
            {
                break;
            }
        }

        // Horizontal
        for (int x = (int)brickCoord.x; x >= 0; x--)
        {
            if (colourMap[x, tempY] == colourMap[tempX, tempY])
            {
                xBricks = xBricks + 1;
            }
            else
            {
                break;
            }
        }

        for (int x = (int)brickCoord.x; x < 8; x++)
        {
            if (colourMap[x, tempY] == colourMap[tempX, tempY])
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

    // maybe have 2 delete coords. one that gets set in check validity and one that you set after you find the final solution brick.
    void destroySolvedBricks(Vector2 brickCount)
    {

        Vector2 currentValidityNumber;
        Vector2 newValidityNumber = new Vector2(0, 0);

        if (brickCount.x >= 3 || brickCount.y >= 3)
        {
            if (brickCount.x > brickCount.y)
            {
                currentValidityNumber = checkValidity(new Vector2(tempDeleteCoordsX.x, tempDeleteCoordsX.y));

                for (int i = (int)tempDeleteCoordsX.x; i > (int)tempDeleteCoordsX.x - brickCount.x; i--)
                {
                    if ((int)currentValidityNumber.x + (int)currentValidityNumber.y >= (int)checkValidity(new Vector2(i, tempDeleteCoordsX.y)).x + (int)checkValidity(new Vector2(i, tempDeleteCoordsX.y)).y)
                    {
                        // do nothing. biggest value is most valuable.
                        deleteCoordsX = tempDeleteCoordsX;
                        deleteCoordsY = tempDeleteCoordsY;
                    }
                    else
                    {
                        newValidityNumber = new Vector2(i, tempDeleteCoordsX.y);
                        currentValidityNumber = checkValidity(new Vector2(i, tempDeleteCoordsX.y));
                    }
                }
            }
            else if (brickCount.y > brickCount.x)
            {
                currentValidityNumber = checkValidity(new Vector2(tempDeleteCoordsY.x, tempDeleteCoordsY.y));

                for (int i = (int)tempDeleteCoordsY.y; i > (int)tempDeleteCoordsY.y - brickCount.y; i--)
                {
                    if ((int)currentValidityNumber.x + (int)currentValidityNumber.y >= (int)checkValidity(new Vector2(tempDeleteCoordsY.x, i)).x + (int)checkValidity(new Vector2(tempDeleteCoordsY.x, i)).y)
                    {
                        // Correct value.
                        deleteCoordsX = tempDeleteCoordsX;
                        deleteCoordsY = tempDeleteCoordsY;
                    }
                    else
                    {
                        newValidityNumber = new Vector2(tempDeleteCoordsY.x, i);
                        currentValidityNumber = checkValidity(new Vector2(tempDeleteCoordsY.x, i));
                    }
                }
            }
            else 
            {
                // Correct value.
                deleteCoordsX = tempDeleteCoordsX;
                deleteCoordsY = tempDeleteCoordsY;
            }
        }
   
        // double check that it is a possible move.
        if (brickCount.y >= 3)
        {
            for (int i = (int)deleteCoordsY.y; i > ((int)deleteCoordsY.y - brickCount.y); i--)
            {
                timeSinceLastMove = 30.0f;
                brickMap[(int)deleteCoordsY.x, i].GetComponent<BrickManager>().deleteBrick();
                brickMap[(int)deleteCoordsY.x, i] = null;
                hasDestroyed = true;
            }
        }
                
        if (brickCount.x >= 3)
        {
            for (int i = (int)deleteCoordsX.x; i > ((int)deleteCoordsX.x - brickCount.x); i--)
            {
                if (brickMap[i, (int)deleteCoordsX.y] != null)
                {
                    timeSinceLastMove = 30.0f;
                    brickMap[i, (int)deleteCoordsX.y].GetComponent<BrickManager>().deleteBrick();
                    brickMap[i, (int)deleteCoordsX.y] = null;
                    hasDestroyed = true;
                }
            }
        }
    }

    // Sort through and destroy all solved bricks.
    void setDestroy()
    {
        
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (brickMap[x, y] != null)
                {
                    destroySolvedBricks(checkValidity(new Vector2(x, y)));
                }
            }
        }
    }

    //Make bricks fall to the bottom.
    void setFall()
    {
        Vector2 tempPosition = new Vector2(0, 0);
        // go through each position in the brickMap[,]
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                // if the current brick being looked at is null.
                if (brickMap[x,y] == null)
                {
                    // keep going up until you find a brick.
                    for (int j = y; j < 8; j++)
                    {
                        if (brickMap[x,j] != null)
                        {
                            // Move the brick then set the old x,y to be null.
                            brickMap[x, j].transform.position = new Vector2(brickMap[x,j].transform.position.x, brickPositionY + (y * yIncrement));
                            brickMap[x, y] = brickMap[x, j];
                            brickMap[x, j] = null;
                            break;
                        }
                    }
                }
            }
        }
    }
    // For every null position in the brickMap[,] array, fill it with a brick of a random colour.
    void setRefill()
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (brickMap[x,y] == null)
                {
                    Position = new Vector3(brickPositionX + (xIncrement * x), brickPositionY + (yIncrement * y));
                    currentBrick = Instantiate(Brick, Position, Quaternion.identity);
                    currentBrick.GetComponent<BrickManager>().setBrickType(Random.Range(1, 6));

                    brickMap[x, y] = currentBrick;
                }
            }
        }
    }

    void setColourMap()
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                colourMap[x, y] = brickMap[x, y].GetComponent<BrickManager>().getBrickType();
            }
        }
    }

    // For every position, check if there is a possible solution.
    public Vector2 checkPossibleMove()
    {
        int tempInt;

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (y >= 7)
                {
                    // do nothing
                }
                else
                {
                    tempInt = colourMap[x, y];
                    colourMap[x, y] = colourMap[x, y + 1];
                    colourMap[x, y + 1] = tempInt;

                    if ((int)checkColourValidity(new Vector2(x, y + 1)).x >= 3 || (int)checkColourValidity(new Vector2(x, y + 1)).y >= 3)
                    {
                        return new Vector2(x, y);
                    }
                    else
                    {
                        setColourMap();
                        // do nothing
                    }
                }

                if (y <= 0)
                {
                    // do nothing
                }
                else
                {
                    tempInt = colourMap[x, y];
                    colourMap[x, y] = colourMap[x, y - 1];
                    colourMap[x, y - 1] = tempInt;

                    if ((int)checkColourValidity(new Vector2(x, y - 1)).x >= 3 || (int)checkColourValidity(new Vector2(x, y - 1)).y >= 3)
                    {
                        return new Vector2(x, y);
                    }
                    else
                    {
                        setColourMap();
                        // do nothing
                    }
                }

                if (x >= 7)
                {
                    // do nothing
                }
                else
                {
                    tempInt = colourMap[x, y];
                    colourMap[x, y] = colourMap[x + 1, y];
                    colourMap[x + 1, y] = tempInt;

                    if ((int)checkColourValidity(new Vector2(x + 1, y)).x >= 3 || (int)checkColourValidity(new Vector2(x + 1, y)).y >= 3)
                    {
                        return new Vector2(x, y);
                    }
                    else
                    {
                        setColourMap();
                        // do nothing
                    }
                }

                if (x <= 0)
                {
                    // do nothing
                }
                else
                {
                    tempInt = colourMap[x, y];
                    colourMap[x, y] = colourMap[x - 1, y];
                    colourMap[x - 1, y] = tempInt;

                    if ((int)checkColourValidity(new Vector2(x - 1, y)).x >= 3 || (int)checkColourValidity(new Vector2(x - 1, y)).y >= 3)
                    {
                        return new Vector2(x, y);
                    }
                    else
                    {
                        setColourMap();
                        // do nothing
                    }
                }
            }
        }
        newBoard();
        fixNewBoard();
        return new Vector2(0,0);
    }
}