using UnityEngine;
using System.Collections;

public class HairCreator : MonoBehaviour {
    float timer;
    float speed = 1.0f;
    public GameObject goldenHair;
    public GameObject ashenHair;
    private float minTime = 10;
    private float maxTime = 20;
	// Use this for initialization
	void Start () {
        timer = Random.Range(minTime, maxTime);
	}
	
	// Update is called once per frame
	void Update () {
        timer -= Time.deltaTime;
        if(timer < 0)
        {
            GameObject newHair;
            timer = Random.Range(minTime,maxTime);
            if(Random.Range(1,4) < 3)
            {
                newHair = (GameObject)Instantiate(ashenHair, new Vector3(transform.position.x + Random.Range(-7, 7), transform.position.y + Random.Range(0, 0.5f), transform.position.z), Quaternion.identity);
            } else
            {
                newHair = (GameObject)Instantiate(goldenHair, new Vector3(transform.position.x + Random.Range(-7, 7), transform.position.y + Random.Range(0, 0.5f), transform.position.z), Quaternion.identity);
            }
            if (Random.Range(1, 10) > 9)
            {
                newHair.transform.position = new Vector3(newHair.transform.position.x, newHair.transform.position.y, -2.0f);
            }

        }


	}
}
