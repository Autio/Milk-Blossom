using UnityEngine;
using System.Collections;

public class TestSingleton<T> : MonoBehaviour where T : MonoBehaviour {

    // Singleton design pattern --> //
    // Only meant to be on an objct that have one instance. 
    private static TestSingleton<T> _instance;

    // need a property to return the instance
    public static TestSingleton<T> Instance
    {
        get
        {
            // create logic to create the instance
            if (_instance == null)
            {
                GameObject go = new GameObject("GameManager");
                go.AddComponent<TestSingleton<T>>();
            }

            return _instance;
        }
    }
    // <-- //

    public int Score { get; set;}
    public bool Live { get; set; }

    void Awake()
    {

    }

    void Start()
    {
        Score = 10;
    }
    

}
