using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDrag : MonoBehaviour {

    private float startTime;
    private float journeyLength;
    Vector3 startPos;
    float speed = 15f;

    enum dragStates {returning, idle};
    dragStates currentState;

    void Start()
    {
        currentState = dragStates.idle;
    }
    // Dragging player sprites

    void OnMouseDrag()
    {
        Vector3 pos;
        pos = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, transform.position.z);
        transform.position = pos;
    }

    void OnMouseUp()
    {
        // Check the collider beneath
        Debug.Log("Drag ended");
        startPos = transform.position;
        startTime = Time.time;
        journeyLength = Vector3.Distance(transform.position, transform.parent.position);
        //        transform.position = transform.parent.position;
        currentState = dragStates.returning;
        
    }

    void Update()
    {
        if(currentState == dragStates.returning)
        {
            Debug.Log("Returning");
            float distCovered = (Time.time - startTime) * speed;
            float fracJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(startPos, transform.parent.position, fracJourney);

            if(transform.position == transform.parent.position)
            {
                currentState = dragStates.idle;
            }
        }
        

    }
}
