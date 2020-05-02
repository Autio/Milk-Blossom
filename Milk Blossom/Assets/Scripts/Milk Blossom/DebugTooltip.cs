using UnityEngine;
using System.Collections;
using TMPro;

public class DebugTooltip : MonoBehaviour {

    public string debugText;
    public TextMeshPro debugToolTip;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		// Change this to a callback
	    debugToolTip.text = debugText;
	}
}
