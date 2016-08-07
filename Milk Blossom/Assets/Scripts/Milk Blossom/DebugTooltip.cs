using UnityEngine;
using System.Collections;

public class DebugTooltip : MonoBehaviour {

    public string debugText;
    public TextMesh debugToolTip;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        debugToolTip.text = debugText;
	}
}
