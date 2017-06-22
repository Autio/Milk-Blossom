using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbox : Singleton<Toolbox> {
// General place for global variables
    protected Toolbox() { } // guarantees this will only be a singleton because you can't use a constructor

    public string testVar = "What";

    private void Awake()
    {

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
