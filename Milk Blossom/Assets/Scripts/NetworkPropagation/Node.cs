using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {
    public int id;
    public List<Node> neighbours = new List<Node>();
    public List<GameObject> nodeLinks = new List<GameObject>();
    public GameObject nodeObject;
    public GameObject bitObject;
    public int capacity = 100;
    [Range(0, 100)]
    public int load = 0;
    public bool spawner = false;
    float rate = 0.2f;
    float counter;
    float startCountdown = 1.0f;
    void Start()
    {
        rate = Random.Range(0.3f, 2.0f);
        counter = rate;
        bitObject = GameObject.Find("Bit");
    }
    void Update()
    {
        // Don't allow immediately
        if (startCountdown < 0)
        {
            if (spawner)
            {
                counter -= Time.deltaTime;
                if (counter < 0)
                {
                    Debug.Log("Spawning at " + Time.timeSinceLevelLoad);
                    counter = rate;
                    SpawnBit(10.0f);
                }
            }
        } else
        {
            startCountdown -= Time.deltaTime;
        }

    }

    void SpawnBit(float speed)
    {
        Node neighbourNode = null;
        // create a new bit and send it to a neighbour

        // select neighbour
        neighbourNode = neighbours[Random.Range(0, neighbours.Count)];

        // create bit
        GameObject bit = (GameObject)Instantiate(bitObject, transform.position, Quaternion.identity);
        bit.transform.position = nodeObject.transform.position;
        bit.transform.parent = GameObject.Find("Bits").transform;
        bit.GetComponent<Bit>().targetID = neighbourNode.id; // Set target ID

        // Send it towards the neighbour
        bit.AddComponent<Rigidbody2D>();
        bit.GetComponent<Rigidbody2D>().gravityScale = 0;
        bit.GetComponent<Rigidbody2D>().AddForce(new Vector2(neighbourNode.nodeObject.transform.position.x - nodeObject.transform.position.x, neighbourNode.transform.position.y - nodeObject.transform.position.y) * speed);
        
        // destroy on arrival
        // There should be a collider being checked on the receiving node

        // decrement load here
        load -= 1;

        // increment load there, needs to happen on arrival
        neighbourNode.load += 1;
    }


    // How should bit collision work? The bit should understand the ID of the node it's been targeted at
    // Whenever it collides with a trigger which is of the right layer, it goes through all the objects of the node list and checks
    // which one it is, if the id matches, then it triggers that nodes incrementation

}