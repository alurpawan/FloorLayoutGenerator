using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block {
    GameObject block;
    Quaternion MinMaxValues;
    Vector3 Direction;

    //Constructor
    public Block(GameObject gameObject) { block = gameObject; }
    
    //CreateMinMaxValues
    public void CalculateMinMaxValues()
    {
        float MinXDistance = block.transform.position.x - block.transform.localScale.x / 2;
        float MaxXDistance = block.transform.position.x + block.transform.localScale.x / 2;
        float MinYDistance = block.transform.position.y - block.transform.localScale.y / 2;
        float MaxYDistance = block.transform.position.y + block.transform.localScale.y / 2;
        MinMaxValues = new Quaternion(MinXDistance, MaxXDistance, MinYDistance, MaxYDistance);

    }

    //Getters
    public Quaternion GetMinMaxValues() { return MinMaxValues; }
    public Vector3 GetDirection() { return Direction; }
    public GameObject GetBlock() { return block; }

    //Setters
    public void SetDirection(Vector3 direction) { Direction = direction; }

    public Vector3 DoorLocation()
    {
        Vector3 doorLocation = block.transform.position;
        Vector3 doorDirection = Direction * -1;
        doorDirection = new Vector3(doorDirection.x * block.transform.localScale.x, doorDirection.y * block.transform.localScale.y, doorDirection.z * block.transform.localScale.z);
        doorDirection = doorDirection / 2;
        return doorLocation + doorDirection;

    }

}
