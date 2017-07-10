using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCreator : MonoBehaviour {

    public int networkNodes;
    List<Node> nodes = new List<Node>();
    List<LineRenderer> nodeLines = new List<LineRenderer>();

    public GameObject nodeObject;

    public float x;
    public float y;

	// Use this for initialization
	void Start () {
        Transform nodeParent = GameObject.Find("Nodes").transform;
        Transform lineParent = GameObject.Find("Links").transform;
        // create nodes
        for (int i = 0; i < networkNodes; i++)
        {
            GameObject newNodeObject = Instantiate(nodeObject, new Vector2(Random.Range(-x, x), Random.Range(-y, y)), Quaternion.identity);

            Node n = newNodeObject.AddComponent<Node>();
            //Node n = new Node();
            nodes.Add(n);
            n.nodeObject = newNodeObject;
            n.id = i;
            n.spawner = true;
            n.nodeObject.transform.parent = nodeParent;

        }
        // create links
        foreach (Node n in nodes)
        {
            int r = Random.Range(0, nodes.Count);
            for (int i = 0; i < Random.Range(1,4); i++)
            {
                bool selfNeighbour = false;
                // don't be a neighbour to yourself
                foreach(Node o in nodes)
                {
                    if (nodes[r].id == n.id)
                    {
                        selfNeighbour = true;
                    } 
                }
                if (!selfNeighbour)
                {
                    n.neighbours.Add(nodes[r]);
                    
                    Debug.Log("Added node " + nodes[r].id.ToString() + " to node " + n.id.ToString());
                    r = Random.Range(0, nodes.Count);

                }
                for (int j = 0; j < 100; j++)
                {
                    if(n.neighbours.Contains(nodes[r]))
                    {
                        r = Random.Range(0, nodes.Count);
                        
                    } else
                    {
                        break;
                    }
                }

            }
        }

        foreach (Node n in nodes)
        {
            foreach (Node nei in n.neighbours)
            {
                GameObject newLink = new GameObject();
                newLink.AddComponent<LineRenderer>().SetPositions(new Vector3[] { n.nodeObject.transform.position, nei.nodeObject.transform.position });
                newLink.GetComponent<LineRenderer>().SetWidth(0.07f, 0.07f);
                n.nodeLinks.Add(newLink);
                newLink.transform.parent = nodeParent;
            }
        }

	}

    // Update is called once per frame
    void Update()
    {
        foreach (Node o in nodes)
        {

            //o.nodeObject.transform.Translate(new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f)) * Time.deltaTime * 0.5f);

            foreach (Node n in o.neighbours)
            {
                float lineWidth = 3.0f;

                Vector3 start = o.nodeObject.transform.position;
                Vector3 end = n.nodeObject.transform.position;
                Vector3 normal = Vector3.Cross(start, end);
                Vector3 side = Vector3.Cross(normal, end - start);
                side.Normalize();
               
                Vector3 a = start + side * (lineWidth / 2);
                Vector3 b = start + side * (lineWidth / -2);
                Vector3 c = end + side * (lineWidth / 2);
                Vector3 d = end + side * (lineWidth / -2);
                               
                //Debug.DrawLine(o.nodeObject.transform.position, n.nodeObject.transform.position, Color.green);
            }
        }
    }

    public bool CheckNode(int id)
    {
        foreach(Node n in nodes)
        {
            if(n.id == id)
            {
                return true;
            }
        }
        return false;
    }
}
