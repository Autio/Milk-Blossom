using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    // General place for global variables
    protected GameManager() { } // guarantees this will only be a singleton because you can't use a constructor

    public enum states { starting, moving, live, ending, paused }
    public states currentState = states.starting;
    public enum controlOptions { keyboard, mouse, touch };
    public controlOptions[] activeControlOptions = new controlOptions[3];

    private void Awake()
    {
        activeControlOptions[0] = controlOptions.mouse;
        activeControlOptions[1] = controlOptions.touch;

    }// (optional) allow runtime registration of global objects
     /* static public T RegisterComponent<T>() where T : Component
      {
         // return Instance.GetOrAddComponent<T>();
      }// Use this for initialization*/
    [System.Serializable]
    public class Language
    {
        public string current;
        public string lastLang;
    }

}
