using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCreator : MonoBehaviour {

    public int networkNodes;
    List<Node> nodes = new List<Node>();
    public GameObject nodeObject;

    public float x;
    public float y;

	// Use this for initialization
	void Start () {
        // create nodes
        for (int i = 0; i < networkNodes; i++)
        {
            Node n = new Node();
            nodes.Add(n);
            n.nodeObject = Instantiate(nodeObject, new Vector2(Random.Range(-x, x), Random.Range(-y, y)), Quaternion.identity);
            n.id = i;
        }
        // create links
        foreach (Node n in nodes)
        {
            for (int i = 0; i < Random.Range(2,4); i++)
            {
                int r = Random.Range(0, nodes.Count);
                if (!n.neighbours.Contains(nodes[r]))
                {
                    n.neighbours.Add(nodes[r]);
                    Debug.Log("Added node " + nodes[r].id.ToString() + " to node " + n.id.ToString());
                }              
            }
        }
	}

    // Update is called once per frame
    void Update()
    {
        foreach (Node a in nodes)
        {
            foreach (Node n in a.neighbours)
            {
                Debug.DrawLine(a.nodeObject.transform.position, n.nodeObject.transform.position, Color.green);
            }
        }
    }
}
