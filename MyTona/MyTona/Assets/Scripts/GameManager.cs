using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{ 
    public GameObject Brick;
    private GameObject currentBrick;

    // Position of the bottom left most tile
    private float BrickPositionX = -9.0f;
    private float BrickPositionY = -4.0f;

    private GameObject[,] brickMap = new GameObject[8,8];
    private GameObject[,] virtualBrickMap = new GameObject[8,8];

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
        
    }

    void newBoard()
    {
        for (int y = 0; y < 8; ++y)
        {
            for (int x = 0; x < 8; ++x)
            {
                Position = new Vector3(BrickPositionX + (xIncrement * x), BrickPositionY + (yIncrement * y));

                // Instantiate a brick at incrementing positions and set colour.
                currentBrick = Instantiate(Brick, Position, Quaternion.identity);
                currentBrick.GetComponent<BrickManager>().SetBrickType((Random.Range(1, 6)));

                brickMap[x, y] = currentBrick;
                // brickMap[x, y] = currentBrick.GetComponent<BrickManager>().GetBrickType();
            }
        }

        virtualBrickMap = brickMap;
        brickMap[3, 3].GetComponent<BrickManager>().deleteBrick();
        //brickMap[3, 3] = null;
    }
}
