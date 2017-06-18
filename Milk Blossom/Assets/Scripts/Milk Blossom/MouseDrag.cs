using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDrag : MonoBehaviour {

    private float startTime;
    private float journeyLength;
    Vector3 startPos;
    float speed = 15f;
    Transform targetTile;
    Vector3 zOffset = new Vector3(0, 0, -0.5f);
    enum dragStates {returning, movingToTarget, idle};
    dragStates currentState;
    void Start()
    {
        currentState = dragStates.idle;
    }
    // Dragging player sprites

    void OnMouseDrag()
    {

        // only allow dragging for the active player. 
        if (this.enabled)
        {
            Vector3 pos;
            pos = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, transform.position.z);
            transform.position = pos;
        }
    }

    void OnMouseUp()
    {

        Debug.Log("Drag ended");
        // check whether legitimate or not
        startPos = transform.position;
        startTime = Time.time;
        journeyLength = Vector3.Distance(transform.position, transform.parent.position);
        //        transform.position = transform.parent.position;
        currentState = dragStates.returning;
        CheckCollisions();


    }



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
            Debug.Log("Moving to target");
            float distCovered = (Time.time - startTime) * speed;
            float fracJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(startPos, targetTile.position + zOffset, fracJourney);

            if (transform.position == (targetTile.position + zOffset))
            {
                currentState = dragStates.idle;
            }
        }


    }

    void CheckCollisions()
    {
        Collider2D[] arr;
        arr = Physics2D.OverlapCircleAll(transform.position, 0.1f);
        foreach(Collider2D a in arr)
        {
            if(a.transform.tag == "Hex")
            {
                targetTile = a.transform;

                // let's assume the move is legitimate
                currentState = dragStates.movingToTarget;
            }
        }
    }

}
