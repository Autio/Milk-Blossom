using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player {

    [Range(1, 4)]
    public int playerNumber;
    bool AI = false;
    bool alive = true;
    Vector3 cubePosition;
    Vector2 offsetPosition;
    public tile playerTile;
    public GameObject playerGameObject;
    public Transform playerWheelTransform;
    private int points;
    public void AddPoints(int p)
    {
        points += p;
    }
    public int GetPoints()
    {
        return points;
    }

    public void SetAlive(bool t)
    {
        alive = t;
    }
    public bool GetAlive()
    {
        return alive;
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
