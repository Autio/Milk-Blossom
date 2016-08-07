using UnityEngine;
using System.Collections;

public class HexBehaviour : MonoBehaviour {
    float myTime = 0f;
    float dropSpeed = 0.15f;
    bool paused = false;
    bool tileDrop = false;
    float dropCounter = 0.1f;
	// Use this for initialization
	void Start () {
	    iTween.MoveBy(this.gameObject, iTween.Hash("z", 0.15, "easeType", "easeInOutCubic", "loopType", "pingPong", "delay", .02));

        
    }
	
	// Update is called once per frame
	void Update () {
        if(myTime < 2.1f)
            myTime += Time.deltaTime;
            if(myTime > 0.3f)
        {
            transform.FindChild("debugtext").gameObject.GetComponent<DebugTooltip>().debugText = "";
        }
        if (!paused)
        {
            if (myTime > 2.1f)
            {
                iTween.Pause(this.gameObject);
                transform.position = new Vector3(transform.position.x, transform.position.y, 0);
                paused = true;
            }
        }
        if(tileDrop)
        {
            if (dropCounter > 0)
            {
                dropCounter -= Time.deltaTime;
                transform.position += new Vector3(0, 0, Time.deltaTime * dropSpeed);
                transform.localScale *= (1 - (Time.deltaTime * dropSpeed));
            } 
            else
            {
                transform.GetComponent<Renderer>().enabled = false;
                tileDrop = false;
            }
        }
	}

    public void DropTile(float duration)
    {
        if (!tileDrop)
        {
            tileDrop = true;
            dropCounter = duration;
        }
    }
    
}
