using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {
    public int id;
    public List<Node> neighbours = new List<Node>();
    public List<GameObject> nodeLines = new List<GameObject>();
    public GameObject nodeObject;
    public GameObject bit;
    public int capacity = 100;
    [Range(0, 100)]
    public int load = 0;
    bool spawner = true;
    public float rate = 1.0f;
    float counter;

    void Start()
    {
        rate = Random.Range(0.3f, 2.0f);
        counter = rate;
    }
    void Update()
    {
        if (spawner)
        {
            counter -= Time.deltaTime;
            if (counter < 0)
            {
                Debug.Log("Spawning at " + Time.timeSinceLevelLoad);
                counter = rate;
            }
        }

    }

    void SpawnBit()
    {
        Node neighbour = null;
        // create a new bit and send it to a neighbour

        // select neighbour

        // create bit
        GameObject bit = new GameObject();

        // destroy on arrival

        // decrement load here
        load -= 1;

        // increment load there, needs to happen on arrival
        neighbour.load += 1;
    }
}
