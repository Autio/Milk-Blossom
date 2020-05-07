using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MoveablePiece {

    [Range(1, 4)]
    public int playerNumber; // Which player does this unit belong to? Number, not index
    public int unitNumber; // Each player can have multiple units
    bool placed = false;
    bool AI = false;
    bool alive = true;
    Vector3 cubePosition;
    Vector2 offsetPosition;
    public Tile playerTile;
    public GameObject playerGameObject;
    public Transform playerWheelTransform;
    public static int[] points = { 0, 0, 0, 0 };

    public void AddPoints(int p)
    {
        points[playerNumber-1] += p;
    }
    public int GetPoints()
    {
        return points[playerNumber - 1];
    }

    public void SetAlive(bool t)
    {
        alive = t;
    }
    public bool GetAlive()
    {
        return alive;
    }

    public void SetPlaced(bool t)
    {
        placed = t;
    }
    public bool GetPlaced()
    {
        return placed;
    }

    public void DeathThroes()
    {
        // make some deathanimation happen
        try
        {
            playerGameObject.transform.localScale *= 2.0f;
        }

        catch
        {

        }
    }

    public bool GetAI()
    {
       
        return AI;
    }

    public void SetAI(bool AIFlag)
    {
        AI = AIFlag;
    }

}
