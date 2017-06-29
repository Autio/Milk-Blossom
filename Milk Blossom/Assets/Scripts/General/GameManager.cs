using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    // General place for global variables
    protected GameManager() { } // guarantees this will only be a singleton because you can't use a constructor

    // GLOBAL VARIABLES
    public enum states { starting, moving, live, ending, paused }
    public states currentState = states.starting;
    public enum controlOptions { keyboard, mouse, touch };
    public controlOptions[] activeControlOptions = new controlOptions[3];

    // MAIN LIST OF ALL TILES ON BOARD
    public static List<tile> tileList = new List<tile>(); // master list of tiles

    Vector3[] directions = new Vector3[6];
    [Range(0, 5)]
    private int currentDir;

    private void Awake()
    {
        activeControlOptions[0] = controlOptions.mouse;
        activeControlOptions[1] = controlOptions.touch;
        
    }

    [System.Serializable]
    public class Language
    {
        public string current;
        public string lastLang;
    }

    // Hex tile helper
    Vector3 CubeDirection(int dir)
    {
        return directions[dir];
    }

    Vector3 CubeNeighbour(tile hex, int dir)
    {
        Vector3 newPosition = hex.cubePosition + directions[dir];
        return newPosition;
    }
}
