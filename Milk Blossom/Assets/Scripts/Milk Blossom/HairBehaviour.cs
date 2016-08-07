using UnityEngine;
using System.Collections;

public class HairBehaviour : MonoBehaviour {
    public float speed = 1.0f;
	// Use this for initialization
	void Start () {
        // randomise size
        this.transform.localScale -= new Vector3(Random.Range(0.1f, 0.6f), 0, 0);

        // cleanup
        Destroy(this.gameObject, 45.0f);
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);

    }
}
