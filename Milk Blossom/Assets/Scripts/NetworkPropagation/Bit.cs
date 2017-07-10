using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bit : MonoBehaviour {
    public int targetID; // sole life purpose of the bit is to reach this
    GameObject NetworkCreator;
    private void Start()
    {
        NetworkCreator = GameObject.Find("Main Camera");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Colliding");
        if(collision.tag == "Node")
        {
            Debug.Log("Colliding With Node");
            if(NetworkCreator.GetComponent<NetworkCreator>().CheckNode(targetID))
            {

                Destroy(this.gameObject);
            }
        }
    }
}
