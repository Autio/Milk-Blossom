using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drag : ControlManager {

    // Make this a child of the touch manager and handle mouse vs touch more intelligently here. 
    // Don't have separate classes for mouse and touch

    private float startTime;
    private float journeyLength;
    Vector3 startPos;
    float speed = 15f;
    Transform targetTile;
    Vector3 zOffset = new Vector3(0, 0, -0.5f);
    enum dragStates {inactive, returning, movingToTarget, idle};
    dragStates currentState;
    MilkBlossom GameController;
    int sourceTileIndex;
    int targetTileIndex;
    bool touch;


    void Start()
    {
        touch = false;
        currentState = dragStates.idle;
        GameController = GameObject.Find("GameController").GetComponent<MilkBlossom>();
    }

    // MOUSE 
    // Dragging player sprites
    private void OnMouseDown()
    {
        if (!touch)
        {
            sourceTileIndex = GameController.liveHexGrid.GetTileIndexByPos(new Vector2(transform.position.x, transform.position.y), GameManager.tileList);

            Debug.Log("Showing all allowed moves for source tile of index " + sourceTileIndex.ToString());
            try
            {
                GameController.AllAllowedMoves(GameManager.tileList[sourceTileIndex]);
            }
            catch
            {
                Debug.Log("Couldn't draw allowed moves");
            }
        }
    }

    void OnMouseDrag()
    {
        if (!touch)
        {
            // Can only happen if the game is in Live mode
            if (GameManager.Instance.currentState == GameManager.states.live || GameManager.Instance.currentState == GameManager.states.placing)
            {

                // only allow dragging for the active player. 
                if (this.enabled)
                {
                    Vector3 pos;
                    pos = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, transform.position.z);
                    transform.position = pos;
                }
            }
        }
    }

    void OnMouseUp()
    {
        Debug.Log("Mouse drag ended");
        if (!touch)
        {
            // Move during gameplay or placement
            if (GameManager.Instance.currentState == GameManager.states.live || GameManager.Instance.currentState == GameManager.states.placing)
            {
                // check whether legitimate or not
                startPos = transform.position;
                startTime = Time.time;
                journeyLength = Vector3.Distance(transform.position, transform.parent.position);
                //        transform.position = transform.parent.position;
                currentState = dragStates.returning;
                if (CheckMove())
                {
                    currentState = dragStates.movingToTarget;
                    GameController.ClearHighlights();
                }
                else
                {
                    currentState = dragStates.returning;
                }
            }
        }
    }


    // TOUCH CONTROL
    void OnFirstTouch()
    {
        touch = true;
        sourceTileIndex = GameController.liveHexGrid.GetTileIndexByPos(new Vector2(transform.position.x, transform.position.y), GameManager.tileList);

        if (GameManager.Instance.currentState == GameManager.states.live || GameManager.Instance.currentState == GameManager.states.placing)
        {

            // only allow dragging for the active player. 
            if (this.enabled)
            {
                Vector3 pos;
                pos = new Vector3(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position).x, Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position).y, transform.position.z);
                transform.position = pos;
            }
        }
         
    }

    void OnFirstTouchEnded()
    {
        if (touch)
        {
            Debug.Log("Touch drag ended");
            // check whether legitimate or not
            startPos = transform.position;
            startTime = Time.time;
            journeyLength = Vector3.Distance(transform.position, transform.parent.position);
            //        transform.position = transform.parent.position;
            currentState = dragStates.returning;
            if (CheckMove())
            {
                currentState = dragStates.movingToTarget;
            }
            else
            {
                currentState = dragStates.returning;
            }

            // Different logic for initial placement?

        }
        touch = false;
    }

    // GENERAL
    void Update()
    {
        if(currentState == dragStates.returning)
        {
            Debug.Log("Returning");
            float distCovered = (Time.time - startTime) * speed;
            float fracJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(startPos, transform.parent.position + zOffset, fracJourney);

            if(transform.position == (transform.parent.position + zOffset))
            {
                currentState = dragStates.idle;
            }
        }
        if (currentState == dragStates.movingToTarget)
        {
            transform.parent.position = targetTile.position + zOffset; // Move the parent object
            Debug.Log("Moving to target");
            float distCovered = (Time.time - startTime) * speed;
            float fracJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(startPos, targetTile.position + zOffset, fracJourney);

            if (transform.position == (targetTile.position + zOffset))
            {
                
                currentState = dragStates.idle;


                // If this is the deployment phase, only deploy the player and increment accordingly
                if (GameManager.Instance.currentState == GameManager.states.placing)
                {
                    targetTileIndex = GameController.liveHexGrid.GetTileIndexByPos(
                       new Vector2(targetTile.transform.position.x, targetTile.transform.position.y),
                       GameManager.tileList);

                    Debug.Log("Target tile index for player placement " + targetTileIndex.ToString());

                    // What is the player unit index? 
                    GameController.MakePlacement(targetTileIndex, this.gameObject.transform.parent.gameObject);
                }

                // Otherwise the move is complete and the appropriate inc
                if (GameManager.Instance.currentState == GameManager.states.live)
                {
                    targetTileIndex = GameController.liveHexGrid.GetTileIndexByPos(
                        new Vector2(targetTile.transform.position.x, targetTile.transform.position.y), 
                        GameManager.tileList);
                    GameController.MakeMove(sourceTileIndex, targetTileIndex);
                }

            }
        }
    }

    bool CheckMove()
    {
        Collider2D[] arr;
        targetTile = null;
        // set float such that the player has a bit of leeway and prefer cancelling the move rather than doing it to an unintended hex
        arr = Physics2D.OverlapCircleAll(transform.position, 0.03f);
        foreach(Collider2D a in arr)
        {
            if (a.transform.tag == "Tile")
            {
                targetTile = a.transform;

                // Check with the hex board whether the move is legitimate or not
                // Get tileindex by looking up position
                int i = GameController.liveHexGrid.GetTileIndexByPos(new Vector2(a.transform.position.x, a.transform.position.y), GameManager.tileList);
                Debug.Log("Move to tile " + i.ToString());
                player arg = new player();

                // Try action on the basis of the index
                if (GameController.CheckMove(i))
                {
                    Debug.Log("Checking move validity");
                    return true;
                } else
                {
                    return false;
                }
            }
        }
            return false;
    }
}