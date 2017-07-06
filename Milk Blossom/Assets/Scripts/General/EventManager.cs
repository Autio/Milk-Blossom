using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {

    public delegate void ClickAction();
    public static event ClickAction OnClicked;

    private void OnGUI()
    {
        if(GUI.Button(new Rect(Screen.width / 2 - 50, 5, 100, 30), "Click"))
        {
            if(OnClicked != null)
            {
                OnClicked();
            }
        }
    }

}

public class TeleportTestScript: MonoBehaviour
{
    private void OnEnable()
    {
        {
            EventManager.OnClicked += TurnColor;
        }
    }
    // Always remove the event 
    private void OnDisable()
    {
        EventManager.OnClicked -= TurnColor;
    }

    void TurnColor()
    {
        Color col = new Color(Random.value, Random.value, Random.value);
        transform.GetComponent<Renderer>().material.color = col;
    }

    // if you want to create a dynamic method system that involves more than one class, use event variables instead of delegate variables
}
