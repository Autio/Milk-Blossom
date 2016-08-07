using UnityEngine;
using System.Collections;

public class PointsBehaviour : MonoBehaviour {
    bool rising = true;
    float restingZ = -.5f;
    float speed = 0.5f;
	// Use this for initialization
	void Start () {
        speed = Random.Range(0.6f, 0.9f);
	}
	
	// Update is called once per frame
	void Update () {
	    if(rising)
        {
            if(transform.position.z > restingZ)
            {
                transform.position -= new Vector3(0, 0, Time.deltaTime * speed);
                
            } else
            {
                rising = false;
            }
        }
	}
}
