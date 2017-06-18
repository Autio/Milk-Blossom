using UnityEngine;
using System.Collections;

public class TouchDrag : TouchManager {

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

        TouchInput(GetComponent<Collider2D>());

    }

    void OnFirstTouch()
    {
        try
        {
            Vector3 pos;
            pos = new Vector3(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position).x, Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position).y, transform.position.z);
            transform.position = pos;
        }
        catch
        {

        }
    }
        
}
