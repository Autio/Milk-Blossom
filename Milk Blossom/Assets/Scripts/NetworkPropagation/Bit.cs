using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bit : MonoBehaviour {
    public int targetID; // sole life purpose of the bit is to reach this
    GameObject NetworkCreator;
    float deathDelay = 0.2f;
    private void Start()
    {
        NetworkCreator = GameObject.Find("Main Camera");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Colliding");
        if(collision.transform.tag == "Node")
        {
            Debug.Log("Colliding With Node");
            if(NetworkCreator.GetComponent<NetworkCreator>().CheckNode(targetID, collision.transform))
            {
                Debug.Log("Destroying bit");
                Destroy(this.gameObject, deathDelay);
            }
        }
    }
}
