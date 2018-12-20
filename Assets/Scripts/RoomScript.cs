using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomScript {
    List<GameObject> gameObjectElements;
    List<Block> blockList = new List<Block>();

    public bool IsInGroup(GameObject gameObject)
    {
        Collider2D thisCollider = gameObject.GetComponent<Collider2D>();
        Collider2D groupCollider;
        for(int i = 0;i < gameObjectElements.Count;i++)
        {
            groupCollider = gameObjectElements[i].GetComponent<Collider2D>();
            if (groupCollider.bounds.Intersects(thisCollider.bounds))
                return true;
        }
        return false;
    }

    //Add and Removing New elements
    public void AddToGroup(GameObject gameObject)
    {
        if (gameObjectElements == null)
            gameObjectElements = new List<GameObject>();
        gameObjectElements.Add(gameObject);
    }
    public bool RemoveElement(GameObject gameObject)
    {
        //if (gameObjectElements.Contains(gameObject))
        //{
            gameObjectElements.Remove(gameObject);
            return true;
        //}
        //else
          //  return false;
    }

    //Merging two RoomScripts
    public void Merge(RoomScript secondRoom)
    {
        for (int i = 0; i < secondRoom.gameObjectElements.Count; i++)
        {
            gameObjectElements.Add(secondRoom.gameObjectElements[i]);
        }
    }

    public void CreateBlockList(Quaternion minMaxValues)
    {
        for(int i = 0;i < gameObjectElements.Count;i++)
        {
            Block tempBlock = new Block(gameObjectElements[i]);
            tempBlock.CalculateMinMaxValues();
            Quaternion blockMinMaxValues = tempBlock.GetMinMaxValues();
            if (blockMinMaxValues[0] == minMaxValues[0])
                tempBlock.SetDirection(new Vector3(-1, 0, 0));
            if (blockMinMaxValues[1] == minMaxValues[1])
                tempBlock.SetDirection(new Vector3(1, 0, 0));
            if (blockMinMaxValues[2] == minMaxValues[2])
                tempBlock.SetDirection(new Vector3(0, -1, 0));
            if (blockMinMaxValues[3] == minMaxValues[3])
                tempBlock.SetDirection(new Vector3(0, 1, 0));
            blockList.Add(tempBlock);
        }
    }

    public List<Block> RoomsWithDoors(Quaternion minMaxValues)
    {
        List<Block> doorBlocks = new List<Block>();

        float minXValue = minMaxValues[1];
        float maxXValue = minMaxValues[0];
        float minYValue = minMaxValues[3];
        float maxYValue = minMaxValues[2];

        Block minXBlock = blockList[0];
        Block maxXBlock = blockList[0];
        Block minYBlock = blockList[0];
        Block maxYBlock = blockList[0];

        for (int i = 0;i < blockList.Count;i++)
        {
            if((blockList[i].GetDirection() == new Vector3(-1,0,0)) && blockList[i].GetMinMaxValues()[1] > maxXValue)
            {
                maxXValue = blockList[i].GetMinMaxValues()[1];
                maxXBlock = blockList[i];
            }

            if ((blockList[i].GetDirection() == new Vector3(1, 0, 0)) && blockList[i].GetMinMaxValues()[0] < minXValue)
            {
                minXValue = blockList[i].GetMinMaxValues()[0];
                minXBlock = blockList[i];
            }

            if ((blockList[i].GetDirection() == new Vector3(0, -1, 0)) && blockList[i].GetMinMaxValues()[3] > maxYValue)
            {
                maxYValue = blockList[i].GetMinMaxValues()[3];
                maxYBlock = blockList[i];
            }

            if ((blockList[i].GetDirection() == new Vector3(0, 1, 0)) && blockList[i].GetMinMaxValues()[2] < minYValue)
            {
                minYValue = blockList[i].GetMinMaxValues()[2];
                minYBlock = blockList[i];
            }

        }

        if (minXValue != minMaxValues[1])
            doorBlocks.Add(minXBlock);
        if (maxXValue != minMaxValues[0])
            doorBlocks.Add(maxXBlock);
        if (minYValue != minMaxValues[3])
            doorBlocks.Add(minYBlock);
        if (maxYValue != minMaxValues[2])
            doorBlocks.Add(maxYBlock);

        return doorBlocks;
    }

   
   }
