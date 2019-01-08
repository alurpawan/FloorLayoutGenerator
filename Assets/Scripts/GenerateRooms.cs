using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct MoveDirection
{
    public float DistanceToMove;
    public Vector3 DirectionToMove;
}

public class GenerateRooms : MonoBehaviour
{

    #region Variables
    //Circle where blocks are placed
    [SerializeField]
    int radius = 20;

    //Block Variables
    [SerializeField]
    int NumberOfBlocks = 10;
    int MinBlockLength = 5;
    int MaxBlockLength = 10;
    List<GameObject> BlockList = new List<GameObject>();
    List<GameObject> WallList = new List<GameObject>();
    [SerializeField]
    GameObject Block;
    [SerializeField]
    GameObject Wall;
    [SerializeField]
    GameObject FloorWall;

    //Room Variables
    List<RoomScript> RoomList = new List<RoomScript>();

    //Door Variables
    List<GameObject> DoorList = new List<GameObject>();
    [SerializeField]
    GameObject Door;

    //Floor and Exit variables
    [SerializeField]
    GameObject Floor;
    [SerializeField]
    GameObject Exit;
    GameObject tempFloor;
    GameObject tempExit;

    //Other Variables
    Quaternion MinMaxValues;
    int numberOfIterations = 0;
    [SerializeField]
    int maxIterations = 10;
    #endregion

    /// <summary>
    /// MAIN FUNCTION : Calls functions in a specific order to generate Layouts
    /// </summary>
    public void GenerateLayout()
    {
        DeleteLayout();
        CreateBlocks();
        MoveBlocks();
        CreateGroups();
        CreateDoors();
        CreateFloor();
        CreateWalls();
    }

    public void CreateWalls()
    {
        for (int i = 0; i < BlockList.Count; i++)
        {
            Vector3 tempScale = BlockList[i].transform.localScale;
            Vector3 tempLoc = BlockList[i].transform.position;
            GameObject tempWall = Instantiate(Wall, tempLoc, new Quaternion(0, 0, 0, 0));
            tempScale.x += 1;
            tempScale.y += 1;
            tempWall.transform.localScale = tempScale;
            WallList.Add(tempWall);
        }
    }

    public void CaptureLayout(int index)
    {
        
        string name = "/Captures/SS" + index.ToString() + ".png";
        ScreenCapture.CaptureScreenshot(Application.dataPath + name);

    }

    /// <summary>
    /// Delete Everything in an existing Layout
    /// </summary>
    void DeleteLayout()
    {
        //Ensure that there are no rooms from last runs
        for (int i = 0; i < BlockList.Count; i++)
        {
            GameObject tempBlock = BlockList[i];
            Destroy(tempBlock);
        }
        //Removing all doors
        for (int i = 0; i < DoorList.Count; i++)
        {
            GameObject tempDoor = DoorList[i];
            Destroy(tempDoor);
        }
        for (int i = 0; i < WallList.Count; i++)
        {
            GameObject tempWall = WallList[i];
            Destroy(tempWall);
        }
        //Clearing all lists
        DoorList.Clear();
        BlockList.Clear();
        RoomList.Clear();
        WallList.Clear();
        Destroy(tempFloor);
        Destroy(tempExit);

    }

    /// <summary>
    /// Creating blocks and adding them to the list
    /// </summary>
    void CreateBlocks()
    {

        
        NumberOfBlocks = (int)Random.Range(10, 20);
        Vector2 point;
        for (int i = 0; i < NumberOfBlocks; i++)
        {

            point = Random.insideUnitCircle * radius;
            point.x = (int)point.x;
            point.y = (int)point.y;
            GameObject tempBlock = Instantiate(Block, point, new Quaternion(0, 0, 0, 0));
            Vector3 scale = new Vector3((int)Random.Range(MinBlockLength, MaxBlockLength), (int)Random.Range(MinBlockLength, MaxBlockLength), 1);
            tempBlock.transform.localScale = scale;
            BlockList.Add(tempBlock);
            
        }
        

    }
    /// <summary>
    /// Moving Blocks to give a room like structure
    /// </summary>
    void MoveBlocks()
    {
        MinMaxValues = CalculateMinMaxXYValues();
        for (int i = 0; i < BlockList.Count; i++)
        {
            GameObject tempBlock = BlockList[i];
            MoveDirection moveDirection = MinDistanceToWalls(MinMaxValues, tempBlock);

            tempBlock.transform.position += moveDirection.DirectionToMove * moveDirection.DistanceToMove;

        }
    }


    /// <summary>
    /// Create Rooms out of existing Rooms
    /// </summary>
    void CreateGroups()
    {
        bool isPartOfList;
        for (int i = 0; i < BlockList.Count; i++)
        {
            GameObject tempBlock = BlockList[i];
            isPartOfList = false;
            for (int j = 0; j < RoomList.Count; j++)
            {
                if (RoomList[j].IsInGroup(tempBlock) == true)
                {
                    //Isnt already part of a group
                    if (isPartOfList == false)
                    {
                        isPartOfList = true;
                        RoomList[j].AddToGroup(tempBlock);

                    }
                    //Is part of a group, must delete Room
                    else
                    {
                        if (RoomList[j].RemoveElement(tempBlock))
                        {
                            BlockList.Remove(tempBlock);
                            Destroy(tempBlock);
                        }
                        else
                            print("Error, Cannot Find Element in RoomList");

                    }
                }
            }
            if (isPartOfList == false)
            {

                RoomScript roomScript = new RoomScript();
                roomScript.AddToGroup(tempBlock);
                RoomList.Add(roomScript);
            }
        }
    }
    /// <summary>
    /// Create Doors for Rooms
    /// </summary>
    void CreateDoors()
    {
        for (int i = 0; i < RoomList.Count; i++)
        {
            RoomList[i].CreateBlockList(MinMaxValues);
            List<Block> blockList = RoomList[i].RoomsWithDoors(MinMaxValues);
            if (blockList.Count == 0)
                print("Hello");
            for (int j = 0; j < blockList.Count; j++)
            {
                Vector3 doorLocation = blockList[j].DoorLocation();
                GameObject tempDoor = (Instantiate(Door, doorLocation, new Quaternion(0, 0, 0, 0)));
                Vector3 Direction = blockList[j].GetDirection();
                if (Direction.x == 0)
                    tempDoor.transform.localScale = new Vector3(2, 1, 1);
                else
                    tempDoor.transform.localScale = new Vector3(1, 2, 1);
                tempDoor.transform.SetParent(blockList[j].GetBlock().transform);
                DoorList.Add(tempDoor);
            }
        }
    }

    void CreateFloor()
    {
        //Generate Floor
        float X_Posn = (MinMaxValues.y - MinMaxValues.x) / 2 + MinMaxValues.x;
        float Y_Posn = (MinMaxValues.w - MinMaxValues.z) / 2 + MinMaxValues.z;
        tempFloor = Instantiate(Floor, new Vector3(X_Posn, Y_Posn, 0), new Quaternion(0, 0, 0, 0));
        tempFloor.transform.localScale = new Vector3((MinMaxValues.y - MinMaxValues.x), (MinMaxValues.w - MinMaxValues.z));

        //Generating Exit Location
        Y_Posn = Y_Posn - (MinMaxValues.w - MinMaxValues.z) / 2 - 3;
        tempExit = Instantiate(Exit, new Vector3(X_Posn, Y_Posn, 0), new Quaternion(0, 0, 0, 0));

        Vector3 tempScale = tempFloor.transform.localScale;
        tempScale.x += 1;
        tempScale.y += 1;
        GameObject tempWall = Instantiate(FloorWall, tempFloor.transform.position, new Quaternion(0, 0, 0, 0));
        tempWall.transform.localScale = tempScale;
        WallList.Add(tempWall);
    }

    //Helper Functions
    Quaternion CalculateMinMaxXYValues()
    {
        //Calculate Minimum and Maximum values of X and Y positions of all blocks
        GameObject MaxX = BlockList[0];
        GameObject MinX = BlockList[0];
        GameObject MaxY = BlockList[0];
        GameObject MinY = BlockList[0];

        float MinXDistance = MinX.transform.position.x - MinX.transform.localScale.x / 2;
        float MaxXDistance = MaxX.transform.position.x + MaxX.transform.localScale.x / 2;
        float MinYDistance = MinY.transform.position.y - MinY.transform.localScale.y / 2;
        float MaxYDistance = MaxY.transform.position.y + MaxY.transform.localScale.y / 2;

        for (int i = 0; i < BlockList.Count; i++)
        {
            GameObject tempBlock = BlockList[i];
            if ((tempBlock.transform.position.x - tempBlock.transform.localScale.x / 2) < MinXDistance)
                MinXDistance = (tempBlock.transform.position.x - tempBlock.transform.localScale.x / 2);
            if ((tempBlock.transform.position.x + tempBlock.transform.localScale.x / 2) > MaxXDistance)
                MaxXDistance = (tempBlock.transform.position.x + tempBlock.transform.localScale.x / 2);
            if ((tempBlock.transform.position.y - tempBlock.transform.localScale.y / 2) < MinYDistance)
                MinYDistance = (tempBlock.transform.position.y - tempBlock.transform.localScale.y / 2);
            if ((tempBlock.transform.position.y + tempBlock.transform.localScale.y / 2) > MaxYDistance)
                MaxYDistance = (tempBlock.transform.position.y + tempBlock.transform.localScale.y / 2);

        }



        return new Quaternion(MinXDistance, MaxXDistance, MinYDistance, MaxYDistance);
    }
    MoveDirection MinDistanceToWalls(Quaternion MinMaxValues, GameObject block)
    {

        float BlockMinX = Mathf.Abs((block.transform.position.x - block.transform.localScale.x / 2) - MinMaxValues.x);
        float BlockMaxX = Mathf.Abs((block.transform.position.x + block.transform.localScale.x / 2) - MinMaxValues.y);
        float BlockMinY = Mathf.Abs((block.transform.position.y - block.transform.localScale.y / 2) - MinMaxValues.z);
        float BlockMaxY = Mathf.Abs((block.transform.position.y + block.transform.localScale.y / 2) - MinMaxValues.w);
        MoveDirection moveDirection;
        moveDirection.DirectionToMove = new Vector3(0, 0, 0);
        moveDirection.DistanceToMove = 0;

        if (Mathf.Min(BlockMinX, BlockMaxX, BlockMaxY, BlockMinY) == BlockMinX)
        {
            moveDirection.DirectionToMove = new Vector3(-1, 0, 0);
            moveDirection.DistanceToMove = BlockMinX;
        }
        else if (Mathf.Min(BlockMinX, BlockMaxX, BlockMaxY, BlockMinY) == BlockMaxX)
        {
            moveDirection.DirectionToMove = new Vector3(1, 0, 0);
            moveDirection.DistanceToMove = BlockMaxX;
        }
        else if (Mathf.Min(BlockMinX, BlockMaxX, BlockMaxY, BlockMinY) == BlockMinY)
        {
            moveDirection.DirectionToMove = new Vector3(0, -1, 0);
            moveDirection.DistanceToMove = BlockMinY;
        }
        else
        {
            moveDirection.DirectionToMove = new Vector3(0, 1, 0);
            moveDirection.DistanceToMove = BlockMaxY;
        }

        return moveDirection;

    }

    void Start()
    {

        StartCoroutine(ScreenShot());

    }

    IEnumerator ScreenShot()
    {

        while (numberOfIterations < maxIterations)
        {
            yield return new WaitForSeconds(1);
            GenerateLayout();
            CaptureLayout(numberOfIterations);
            numberOfIterations++;
        }
    }


}