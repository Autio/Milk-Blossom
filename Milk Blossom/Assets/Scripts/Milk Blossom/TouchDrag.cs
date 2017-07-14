using UnityEngine;
using System.Collections;

public class TouchDrag : TouchManager {
    // Replicating mouse drag but for touch
    private float startTime;
    private float journeyLength;
    Vector3 startPos;
    float speed = 15f;
    Transform targetTile;
    Vector3 zOffset = new Vector3(0, 0, -0.5f);
    enum dragStates { inactive, returning, movingToTarget, idle };
    dragStates currentState;
    MilkBlossom GameController;
    int sourceTileIndex;
    int targetTileIndex;
    // Use this for initialization
    void Start()
    {
        currentState = dragStates.idle;
        GameController = GameObject.Find("GameController").GetComponent<MilkBlossom>();
    }

    // Update is called once per frame
    void Update() {

        TouchInput(GetComponent<Collider2D>());

        if (currentState == dragStates.returning)
        {
            Debug.Log("Returning");
            float distCovered = (Time.time - startTime) * speed;
            float fracJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(startPos, transform.parent.position + zOffset, fracJourney);

            if (transform.position == (transform.parent.position + zOffset))
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

                targetTileIndex = GameController.liveHexGrid.GetTileIndexByPos(new Vector2(targetTile.transform.position.x, targetTile.transform.position.y), GameManager.tileList);

                // The move is complete and the appropriate inc
                GameController.MakeMove(sourceTileIndex, targetTileIndex);
            }
        }
    }

    void OnFirstTouch()
    {
        try
        {
            if (GameManager.Instance.currentState == GameManager.states.live)
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
        catch
        {

        }
    }

    void OnFirstTouchEnded()
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


    }

    bool CheckMove()
    {
        Collider2D[] arr;
        targetTile = null;
        // set float such that the player has a bit of leeway and prefer cancelling the move rather than doing it to an unintended hex
        arr = Physics2D.OverlapCircleAll(transform.position, 0.03f);
        foreach (Collider2D a in arr)
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
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        return false;
    }

}
