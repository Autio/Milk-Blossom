﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;


public class MilkBlossom : MonoBehaviour
{
    // TODO 
    // Mouse & Touch controls 
    // An android deployment

    // Making a Hey That's My Fish variant
    // Basic gameplay: Hex grid
    // Each hex has 1 to 3 points
    // Player has one unit to control (could have more)
    // When they LEAVE a tile, they pick up the points
    // The tile that is left gets removed from play
    // On their turn they can move in a straight unobstructed line as long a distance as they like
    // Players cannot move over empty tiles or tiles with other player units
    // The player who has most points once not more moves can be made wins 

    // In order to make this game I need to implement:
    // A hex grid                                                           x
    // Points on the grids                                                  x
    // States for starting and player turn selection and turn movement      x
    // Player characters                                                    x
    // Points counting                                                      x
    // Timer    
    // Overall aesthetic

    // AI for enemies: 
    // first scan straight line options
    // for the tiles with three points, also check the next move for whether there are more
    // tiles with three points available, in which case prioritise those for the move
    // if no three point tiles are available, do the same for two point tiles, 
    // else do it for one point tiles
                                                            

    // further ideas
    // just let players do as many turns as feasible, don't worry about isolated areas (though
    // they need to be taken into account in calculating when the game ends?                x
    // game ends when a player has no further valid moves                                   x
    // player who can't move does a shaky-shake and perishes
    // music tied to the tiles the players are on
    // What about an overall level handler? How do you do progression and keep the player interested for multiple games? 
    // What about difficulty handling?

    
    // MAIN GAME LOGIC
    Camera mainCam;
    private enum states { starting, planning, live, moving, ending, paused };
    states currentState = states.starting;
    bool firstTurn = true;
    Vector3[] directions = new Vector3[6];
    [Range(0, 5)]
    private int currentDir;
    public enum controlOptions { keyboard, mouse, touch };
    public controlOptions controlOption = controlOptions.mouse;

    public GameObject[] pointsObjects;
    public GameObject hexTile;
    public GameObject[] scoreObjects;
    private GameObject timerBar;
    public bool timed = false; // do human players have limited time to do turns or not


    // Tile map attributes
    hexGrid liveHexGrid;
    public int hexGridx = 5;
    public int hexGridy = 5;
    public float hexRadius = 0.5f;
    public int hexGridRadius = 3;
    private int targetRange = 2;
    private float turnCooldown = 0.5f;
    private float moveCoolDown = 2.0f;
    public float turnTimeLimit = 10f;
    private float turnTimeCounter = 0;
    public bool useAsInnerCircleRadius = true;
    static List<tile> tileList = new List<tile>(); // master list of tiles
    static tile activeTile; // does it make sense to keep the active tile as a variable like this?

    // player info, would be better in a class probably. Limiting players to 4
    public GameObject playerObject;
    [Range(1, 4)]
    public int players = 3;
    public int AIPlayers = 3;
    static List<player> playerList = new List<player>();
    public int activePlayer = 0;

    // ART & VISUAL
    static Sprite[] tileSprites;
    public static Color[] highlightColorList = new Color[3]; // 0 for source, 1 for midway and 2 for target
    public Color[] highlightColorsListPublic;
    public GameObject endText;

    // Use this for initialization
    void Start()
    {

        for (int c = 0; c < 3; c++)
        {
            highlightColorList[c] = highlightColorsListPublic[c];
        }

        mainCam = GameObject.Find("Main Camera").GetComponent<Camera>();
        
        // Only if we need timed turns
        timerBar = GameObject.Find("TimerBar");

        // Direction mapping
        directions[0] = new Vector3(+1, -1, 0);
        directions[1] = new Vector3(+1, 0, -1);
        directions[2] = new Vector3(0, +1, -1);
        directions[3] = new Vector3(-1, +1, 0);
        directions[4] = new Vector3(-1, 0, +1);
        directions[5] = new Vector3(0, -1, +1);

        InitGame();

    }
    // CLASSES
    public class player
    {
        [Range(1, 4)]
        public int playerNumber;
        bool AI = false;
        bool alive = true;
        Vector3 cubePosition;
        Vector2 offsetPosition;
        public tile playerTile;
        public GameObject playerGameObject;
        public Transform playerWheelTransform;
        private int points;
        public void AddPoints(int p)
        {
            points += p;
        }
        public int GetPoints()
        {
            return points;
        }

        public void SetAlive(bool t)
        {
            alive = t;
        }
        public bool GetAlive()
        {
            return alive;
        }

        public void DeathThroes()
        {
            // make some deathanimation happen
            try
            {
                playerGameObject.transform.localScale *= 2.0f;
            }

            catch
            {

            }
        }

        public bool GetAI()
        {
            return AI;
        }

        public void SetAI(bool AIFlag)
        {
            AI = AIFlag;
        }
    }

    public class tile
    {
        public Vector3 cubePosition;
        public Vector2 offsetPosition;
        [Range(1, 3)]
        public int points;
        public int index;
        bool active = true;
        public bool occupied = false;
        bool highlighted = false;
        public int highlightColor;
        public GameObject tileObject;
        public int[] moveValues = new int[4];
        public GameObject tilePointsObject;
        void drawPoints()
        {
            //
        }

        public void SetHighlight(bool isOn)
        {
            highlighted = isOn;
            if (highlighted)
            {
                tileObject.GetComponent<Renderer>().material.color = highlightColorList[highlightColor];
            }
            else
            {
                try
                {
                    tileObject.GetComponent<Renderer>().material.color = Color.white;
                }
                catch
                {
                    Debug.Log("Couldn't clear highlight");
                }
            }
        }
        public bool GetHighlight()
        {
            return highlighted;
        }

        public void SetOccupied(bool occupyFlag)
        {
            occupied = occupyFlag;
        }
        public bool GetOccupied()
        {
            return occupied;
        }

        public void SetActive(bool activeFlag)
        {
            active = activeFlag;

            if (!active)
            {
                //tileObject.GetComponent<HexBehaviour>().DropTile(12f);
                tilePointsObject.SetActive(false);

            }


        }

        public bool GetActive()
        {
            return active;
        }

        private IEnumerator DisableRenderer(GameObject o, float delay)
        {
            yield return new WaitForSeconds(delay);
            o.GetComponent<Renderer>().enabled = false;
        }
    }
    static IEnumerator basicDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
    }

    public class hexGrid
    {
        // what needs to be public here?
        public int x = 5;
        public int y = 5;
        public float radius = 0.5f;
        public bool useAsInnerCircleRadius = true;
        int tileCount = 0;
        public int playerCount = 3;
        public int AIPlayerCount;
        public GameObject playerObj;
        public GameObject[] pointsObjects;

        private float offsetX, offsetY;
        private float standardDelay = 0.03f;
        private string superSecretMessage = "Videogames Rot Your Brains Videogames Rot Your Brains Videogames Rot Your Brains";
        // list of positions
        new Vector3 maxBounds = new Vector3(0, 0, 0);
        new Vector3 minBounds = new Vector3(0, 0, 0);

        public IEnumerator CreateHexShapedGrid(GameObject hexTile, int gridRadius = 3)
        {

            float delayModifier = 1.0f;
            float unitLength = (useAsInnerCircleRadius) ? (radius / Mathf.Sqrt(3) / 2) : radius;

            offsetX = unitLength * Mathf.Sqrt(3);
            offsetY = unitLength * 1.5f;

            // create the tilemap in the shape of a hexagon

            for (int q = -gridRadius; q <= gridRadius; q++)
            {
                int r1 = Mathf.Max(-gridRadius, -q - gridRadius);
                int r2 = Mathf.Min(gridRadius, -q + gridRadius);
                for (int r = r1; r <= r2; r++)
                {
                    // instantiate tile
                    tile newTile = new tile();
                    newTile.index = tileCount;
                    tileList.Add(newTile);
                    tileCount++;

                    // Place the tile
                    newTile.cubePosition = new Vector3(q, r, -q - r);
                    Vector2 offset = CubeToOddR(newTile.cubePosition);
                    Vector2 hexPos = HexOffset((int)offset.x, (int)offset.y);
                    Vector3 pos = new Vector3(hexPos.x, hexPos.y, 0);
                    newTile.offsetPosition = pos;

                }

            }
            for (int t = 0; t < tileList.Count; t++)
            {

                GameObject newTileObject = (GameObject)Instantiate(hexTile, tileList[t].offsetPosition, Quaternion.identity);
                tileList[t].tileObject = newTileObject;
                // Set Tile Sprite 
                try

                {
                    int spriteType = Random.Range(0, tileSprites.Length);
                    tileList[t].tileObject.GetComponent<SpriteRenderer>().sprite = tileSprites[spriteType];
                  
                }
                catch
                {
                    Debug.Log("Sprite for tile could not be set");

                }
                tileList[t].tileObject.transform.parent = GameObject.Find("HexGrid").transform;
                tileList[t].tileObject.transform.FindChild("debugtext").gameObject.GetComponent<DebugTooltip>().debugText = superSecretMessage[t].ToString();
                yield return new WaitForSeconds(standardDelay);
            }

            // do remaining setup things within ienumerator to ensure sequence is correct
            // 2: Is this the correct place? 
            AllocatePoints();
            yield return new WaitForSeconds(1.4f);
            AllocatePlayers(playerObj);

            // Set starting player 
            // 2: This shouldn't always be the human player, should it?
            activeTile = SelectPlayer(0);

        }

        // Place the points onto the tiles at the start of a level 
        public void AllocatePoints()
        {

            int[] pool = new int[3];
            for (int i = 0; i < pool.Length; i++)
            // if there are 60 tiles, 10 are triples, 20 are doubles and 30 are singles, i.e. 1/6, 1/3 and 1/2
            // 2: Does this place a constraint on the tile count? Are odd numbers allowed, for example?
            {
                pool[i] = Mathf.FloorToInt(tileCount / (2 + i * i));
            }
            pool[0] += tileCount - pool[0] - pool[1] - pool[2];

            List<tile> choosableTiles = new List<tile>(tileList);
            for (int t = 0; t < tileList.Count; t++)
            {

                // randomly choose tile that hasn't been chosen before
                tile chosenTile = choosableTiles[Random.Range(0, choosableTiles.Count - 1)];
                int attempts = 20;
                // create points in tiles
                while (chosenTile.points <= 0)
                {


                    int r = Random.Range(0, pool.Length);
                    for (int k = 0; k < pool.Length; k++)
                    {
                        if (r == k)
                        {
                            if (pool[k] > 0)
                            {
                                pool[k] -= 1;
                                // this tile gets 1 point
                                chosenTile.points = k + 1;
                                // Instantiate points object
                                float zOffset = 2f;
                                GameObject pointsObject = (GameObject)Instantiate(pointsObjects[k], new Vector3(chosenTile.tileObject.transform.position.x, chosenTile.tileObject.transform.position.y, zOffset), Quaternion.identity);
                                chosenTile.tilePointsObject = pointsObject;
                                // AddDebugText(chosenTile.tileObject, chosenTile.points.ToString());
                                choosableTiles.Remove(chosenTile);

                            }
                        }
                    }
                    attempts -= 1;
                    if (attempts < 0)
                    {
                        break;
                    }
                }
            }
        }
        public void AllocatePlayers(GameObject playerObject)
        {
            // legitimate tiles are the ones with one point only
            List<tile> validAllocationTiles = new List<tile>();

            foreach (tile t in tileList)
            {
                if (t.points == 1)
                {
                    validAllocationTiles.Add(t);
                }
            }

            for (int p = 1; p <= playerCount; p++)
            {
                int attempts = 20;
                while (true)
                {
                    attempts -= 1;
                    if (attempts < 0)
                    {
                        break;
                    }
                    tile chosenTile = validAllocationTiles[Random.Range(0, validAllocationTiles.Count)];
                    if (!chosenTile.GetOccupied())
                    {
                        // allocate player on tile
                        Debug.Log("allocating player");
                        //yield return new WaitForSeconds(standardDelay);
                        GameObject newPlayer = (GameObject)Instantiate(playerObject, new Vector3(chosenTile.tileObject.transform.position.x, chosenTile.tileObject.transform.position.y, -0.5f), Quaternion.identity);
                        // set player text
                        newPlayer.transform.FindChild("PlayerSprite").transform.FindChild("PlayerLabel").GetComponent<TextMesh>().text = "P" + p.ToString();
                        chosenTile.SetOccupied(true);
                        player pl = new player();
                        playerList.Add(pl);
                        pl.playerNumber = p;
                        pl.playerTile = chosenTile;
                        pl.playerGameObject = newPlayer;
                        pl.playerWheelTransform = newPlayer.transform.FindChild("PlayerWheels");

                        if (playerCount - p < AIPlayerCount)
                        {
                            pl.SetAI(true);
                            Debug.Log("Player " + p.ToString() + " set to AI");
                        }

                        if (p == 1)
                        {
                            if (!pl.GetAI())
                            {
                                pl.playerGameObject.transform.FindChild("PlayerSprite").GetComponent<TouchDrag>().enabled = true;
                            }
                        }

                        break;
                    }
                }
            }




        }

        public void CreateGrid(GameObject hexTile)
        {
            tileCount = x * y;
            // Would there need to be a variety of methods here depending on the hex grid type?

            // create tiles themselves
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    Vector2 hexPos = HexOffset(i, j);
                    tile newTile = new tile();

                    // convert coords to cube format based on whether row amount is odd or even
                    newTile.cubePosition = OddRToCube(i, j);
                    // randomly choose points amount
                    tileList.Add(newTile);
                    Vector3 pos = new Vector3(hexPos.x, hexPos.y, 0);
                    GameObject newTileObject = (GameObject)Instantiate(hexTile, pos, Quaternion.identity);
                    newTile.tileObject = newTileObject;
                    try
                    {
                        newTileObject.transform.parent = GameObject.Find("HexGrid").transform;

                    }
                    catch
                    {
                        Debug.Log("Was not able to add hex to hex grid");
                    }

                    // AddDebugText(newTileObject, newTile.cubePosition.x.ToString() + "," + newTile.cubePosition.y.ToString() + "," + newTile.cubePosition.z.ToString());

                }
            }
        }

        // Tidy up activity when leaving the tile
        public void leaveTile(tile tileToLeave)
        {
            tileToLeave.SetOccupied(false);
            tileToLeave.SetActive(false);
            tileToLeave.tileObject.SetActive(false);

        }

        public void enterTile(tile tileToEnter)
        {

            tileToEnter.SetOccupied(true);
        }



        Vector2 HexOffset(int x, int y)
        {
            Vector2 position = Vector2.zero;

            if (y % 2 == 0)
            {
                position.x = x * offsetX;
                position.y = y * +offsetY;
            }
            else
            {
                position.x = (x + 0.5f) * offsetX;
                position.y = y * +offsetY;
            }
            return position;
        }

        // hex tile position conversion helper functions http://www-cs-students.stanford.edu/~amitp/
        // odd r to cube
        Vector3 OddRToCube(int x, int y)
        {
            Vector3 cubeCoordinates = new Vector3();
            cubeCoordinates.x = x - (y - (y & 1)) / 2;
            cubeCoordinates.z = y;
            cubeCoordinates.y = -cubeCoordinates.x - cubeCoordinates.z;

            return cubeCoordinates;
        }

        // cube to odd r
        Vector2 CubeToOddR(Vector3 cubeCoords)
        {
            Vector2 oddR = new Vector2();
            oddR.x = (int)cubeCoords.x + ((int)cubeCoords.z - ((int)cubeCoords.z & 1)) / 2;
            oddR.y = (int)cubeCoords.z;

            return oddR;
        }

        // even r to cube
        Vector3 EvenRToCube(int x, int y)
        {
            Vector3 cubeCoordinates = new Vector3();
            cubeCoordinates.x = x - (y + (y & 1)) / 2;
            cubeCoordinates.z = y;
            cubeCoordinates.y = -cubeCoordinates.x - cubeCoordinates.z;

            return cubeCoordinates;
        }


        // DEBUG METHODS
        void AddDebugText(GameObject targetObject, string inputText)
        {
            try
            {
                //string existingText = targetObject.transform.FindChild("debugtext").gameObject.GetComponent<DebugTooltip>().debugText;
                targetObject.transform.FindChild("debugtext").gameObject.GetComponent<DebugTooltip>().debugText = inputText;
            }
            catch
            {
                Debug.Log("Failed to add text");
            }

        }
        public void DisplayIndices()
        {
            // simply number tiles
            for (int i = 0; i < tileList.Count; i++)
            {

                AddDebugText(tileList[i].tileObject, i.ToString());
            }
        }
        public void DisplayCoords()
        {
            // cubic coords
            for (int i = 0; i < tileList.Count; i++)
            {
                string coordText = tileList[i].cubePosition.x.ToString() + ", " + tileList[i].cubePosition.y.ToString() + ", " + tileList[i].cubePosition.z.ToString();
                AddDebugText(tileList[i].tileObject, coordText);
            }
        }
        public void DisplayPoints()
        {
            // points
            for (int i = 0; i < tileList.Count; i++)
            {
                AddDebugText(tileList[i].tileObject, tileList[i].points.ToString());
            }
        }

        public void DisplayMoveValues(int a)
        {
            for (int i = 0; i < tileList.Count; i++)
            {
                AddDebugText(tileList[i].tileObject, tileList[i].moveValues[a].ToString());
            }
        }

        public void DisplayClear()
        {
            // clear all
            for (int i = 0; i < tileList.Count; i++)
            {
                AddDebugText(tileList[i].tileObject, "");
            }
        }
    }

    void Awake()
    {

        tileSprites = (Sprite[])Resources.LoadAll<Sprite>("Sprites\\HexSprite2");
    }

   

    public void SetPlayerDraggability()
    {
        foreach (player p in playerList)
        {
            if (p.playerNumber != (activePlayer + 1))
            {
                p.playerGameObject.transform.FindChild("PlayerSprite").GetComponent<TouchDrag>().enabled = false;
            }
            else
            {
                if (p.GetAI())
                {
                    p.playerGameObject.transform.FindChild("PlayerSprite").GetComponent<TouchDrag>().enabled = false;
                }
                else
                {
                    p.playerGameObject.transform.FindChild("PlayerSprite").GetComponent<TouchDrag>().enabled = true;
                }
            }
        }
    }


    void IncrementActivePlayer()
    {
        activePlayer++;
        if (activePlayer >= players)
        {
            activePlayer = 0;
        }
        activeTile = SelectPlayer(activePlayer);

        // Only the current player can be dragged and dropped, if it's not an AI
        SetPlayerDraggability();

        // Show all possible moves when dragging & dropping
        AllAllowedMovesHighlighter(activeTile);


        if (!ValidMoves(playerList[activePlayer]))
        {
            Debug.Log("No valid moves for player " + (activePlayer + 1).ToString());
            if (playerList[activePlayer].GetAlive())
            {
                // can't make moves
                // player is taken out of circulation
                playerList[activePlayer].SetAlive(false);
                playerList[activePlayer].DeathThroes();
            }
            // does this mean all players are dead?
            if (CheckPlayersAlive())
            {
                IncrementActivePlayer();
            }
            else
            {
                // transition to end state
                StartCoroutine(switchState(states.ending, 2.0f));
            }

        }

        /*
                // if player is AI, do an AI move
                if(playerList[activePlayer - 1].GetAI())
                {
                    // 
                    AIMove(playerList[activePlayer - 1]);
                    switchState(states.moving, 2.0f);
                }
        */

    }

    private bool CheckPlayersAlive()
    {
        for (int p = 0; p < players; p++)
        {
            if (playerList[p].GetAlive())
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator SetupPlayers()
    {

        for (int p = 0; p < players; p++)
        {

        }
        yield return new WaitForSeconds(1.0f);
    }

    IEnumerator Wait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        AllAllowedMovesHighlighter(activeTile);
    }

    void InitGame()
    {
        // create grid, allocate points and allocate players
        liveHexGrid = new hexGrid();
        liveHexGrid.x = hexGridx;
        liveHexGrid.y = hexGridy;
        liveHexGrid.radius = hexRadius;
        liveHexGrid.useAsInnerCircleRadius = useAsInnerCircleRadius;
        liveHexGrid.playerCount = players;
        liveHexGrid.AIPlayerCount = AIPlayers;
        liveHexGrid.playerObj = playerObject;
        liveHexGrid.pointsObjects = pointsObjects;
        StartCoroutine(liveHexGrid.CreateHexShapedGrid(hexTile, hexGridRadius));

        // once game is setup, set it to live
        StartCoroutine(switchState(states.live, 5.0f));

        // set player amounts
        for (int i = 0; i < players; i++)
        {
            if (i < 2)
            {
                scoreObjects[i].transform.GetComponent<Text>().text = "P" + (i + 1).ToString() + "\n" + "0";
            }
            else
            {
                scoreObjects[i].transform.GetComponent<Text>().text = "0\n" + "P" + (i + 1).ToString();
            }
        }

    }

    IEnumerator switchState(states s, float delay)
    {
        // first pause the game in transition limbo until the delay has passed
        currentState = states.paused;
        yield return new WaitForSeconds(delay);

        // then switch to desired state
        currentState = s;

        Debug.Log("Switced to " + s);

    }

    // Update is called once per frame
    void Update()
    {

        // Debug.Log("active player " + activePlayer);
        // Debug.Log("current state " + currentState);
        // RESET
        if (Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        }

        //        timerBar.transform.GetComponent<ProgressBar.ProgressBarBehaviour>().SetFillerSizeAsPercentage(100);
        //timerBar.transform.GetComponent<ProgressBar.ProgressBarBehaviour>().SetFillerSize();
        // timer before live

        if (currentState == states.live)
        {
            if(firstTurn)
            {
                AllAllowedMovesHighlighter(activeTile);
                firstTurn = false;
            }
            // TURN TIMER
            // only count for human players
            if (timed)
            {
                if (!playerList[activePlayer].GetAI())
                {
                    turnTimeCounter += Time.deltaTime;
                    if (turnTimeCounter > turnTimeLimit)
                    {
                        // Indicate to player that time is up

                        // FORCE MOVE FROM PLAYER
                        // (call AI move)

                        // Increment player turn
                        turnTimeCounter = 0;

                        IncrementActivePlayer();
                        ClearHighlights();

                    }

                }
            }


            // CONTROLS

            // debug visualisations 
            if (Input.GetKey(KeyCode.F1))
            {
                liveHexGrid.DisplayIndices();
            }
            if (Input.GetKey(KeyCode.F2))
            {
                liveHexGrid.DisplayCoords();
            }
            if (Input.GetKey(KeyCode.F3))
            {
                liveHexGrid.DisplayPoints();
            }

            if (Input.GetKey(KeyCode.F4))
            {
                liveHexGrid.DisplayMoveValues(activePlayer);
            }
            if (Input.GetKey(KeyCode.F5))
            {
                liveHexGrid.DisplayClear();
            }
            // DEBUG TURN SWITCHING
            if (Input.GetKey(KeyCode.N))
            {
                IncrementActivePlayer();
            }

            if (Input.GetKey(KeyCode.W))
            {
                targetRange++;
                if (targetRange > hexGridRadius * 2)
                {
                    targetRange = hexGridRadius * 2;
                }
                // dependent on gridradius
            }
            if (Input.GetKey(KeyCode.X))
            {
                targetRange--;
                if (targetRange < 1)
                {
                    targetRange = 1;
                }
            }

            // Rudimentary highlight controls
            if (Input.GetKey(KeyCode.Q))
            {
                currentDir = 4;
            }

            if (Input.GetKey(KeyCode.E))
            {
                currentDir = 5;
            }

            if (Input.GetKey(KeyCode.A))
            {
                currentDir = 3;

            }
            if (Input.GetKey(KeyCode.D))
            {
                currentDir = 0;
            }
            if (Input.GetKey(KeyCode.Z))
            {
                currentDir = 2;
            }
            if (Input.GetKey(KeyCode.C))
            {
                currentDir = 1;
            }


            if (controlOption == controlOptions.mouse)
            {



            }

            tile targetTile = null;

            if (Input.GetKey(KeyCode.T))
            {
                PseudoAIMove(playerList[activePlayer]);
            }

            turnCooldown -= Time.deltaTime;
            if (turnCooldown < 0)
            {
                // check if it's AI's turn to go
                if (playerList[activePlayer].GetAI())
                {
                    turnCooldown = 0.75f;
                    AIMove(playerList[activePlayer]);

                }
                else
                {
                    turnCooldown = 0.03f;

                    //  targetTile = LinearHighlighter(activeTile, currentDir, targetRange);
                }
                

                if (Input.GetKey(KeyCode.Return))
                {
                    if (activeTile != targetTile)
                    {

                        MakeMove(playerList[activePlayer], targetTile);
                        ClearHighlights();

                        // IncrementativePlayer();
                    }
                }

            }

        }

        if (currentState == states.ending)
        {
            int hiPlayer = 0;
            int hiScore = 0;
            for (int i = 0; i < players; i++)
            {
                if (playerList[i].GetPoints() > hiScore)
                {
                    hiPlayer = i + 1;
                    hiScore = playerList[i].GetPoints();
                }

            }

            endText.transform.GetComponent<Text>().text = "ENDED\nPlayer " + hiPlayer.ToString() + "\nWins";
        }
    }

    // if switching to player, check if any valid moves are available
    public bool ValidMoves(player p)
    {
        for (int d = 0; d < directions.Length; d++)
        {
            foreach (tile t in tileList)
            {
                // is there a tile that sits one step in this direction anywhere on the board that
                // is both active and unoccupied
                if (t.cubePosition == p.playerTile.cubePosition + directions[d])
                {
                    if (!t.GetOccupied() && t.GetActive())
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }


    static tile SelectPlayer(int playerNumber = 0)
    {
        Debug.Log("player number " + (playerNumber + 1).ToString() + " selected");
        foreach (player p in playerList)
        {
            Debug.Log(p.playerNumber);
            if (p.playerNumber == (playerNumber + 1))
            {
                Debug.Log("Setting active tile");
                return p.playerTile;
            }
        }
        return null;
    }

    public void ClearHighlights()
    {
        foreach (tile t in tileList)
        {
            t.SetHighlight(false);
        }
        try
        {
            playerList[activePlayer].playerTile.SetHighlight(true);
        }
        catch
        {

        }

    }

    void AllAllowedMovesHighlighter(tile sourceTile)
    {
        // Unhighlight all tiles
        foreach (tile t in tileList)
        {
             t.SetHighlight(false);
        }
        try
        {
            sourceTile.SetHighlight(true);
        }
        catch
        {
            Debug.Log("Could not highlight sourcetile");

        }
        for (int d = 0; d < 6; d++)
        {
            bool directionBlocked = false;
            for (int r = 1; r <= (hexGridRadius * 2); r++)
            {

                Vector3 relativeTargetPosition = directions[d] * r;
                // try and step to the next tile in the direction
                // does the tile exist
                foreach (tile t in tileList)
                {
                    if (t.cubePosition == sourceTile.cubePosition + relativeTargetPosition)
                    {
                        if (!directionBlocked)
                        {
                            if (t.GetActive() && !t.GetOccupied())
                            {
                                t.SetHighlight(true);
                            }
                            else
                            {
                                t.SetHighlight(false);
                                directionBlocked = true;

                                // if it's not a valid tile then it is either deactivated or occupied and the last tile should be the one before the obstacle
                                //range = r; // bad practice, setting an int within the function that's returning a type
                                //targetTile.highlightColor = 2;
                                //return targetTile;

                            }
                        }
                        else
                        {
                            t.SetHighlight(false);
                        }
                    }
                }
            }
        }
    }

    tile LinearHighlighter(tile sourceTile, int direction, int range)
    {
        // The 5 is linked to the board size
        Mathf.Clamp((float)direction, 0, 5);


        // there should be a way to know from the cubic coordinates whether the tile is on a line
        // Refresh the highlighted tiles for all in range
        AllAllowedMovesHighlighter(activeTile);

        // first, unhighlight all tiles
        foreach (tile t in tileList)
        {
            t.SetHighlight(false);
        }

        try
        {
            sourceTile.SetHighlight(true);
        }
        catch
        {

        }

        sourceTile.highlightColor = 0;
        tile targetTile = sourceTile;
        for (int r = 1; r <= range; r++)
        {
            Vector3 relativeTargetPosition = directions[direction] * r;
            // try and step to the next tile in the direction
            // does the tile exist
            foreach (tile t in tileList)
            {
                if (t.cubePosition == sourceTile.cubePosition + relativeTargetPosition)
                {
                    if (t.GetActive() && !t.GetOccupied())
                    {
                        t.SetHighlight(true);
                        targetTile = t;
                        targetTile.highlightColor = 1;
                    }
                    else
                    {
                        // if it's not a valid tile then it is either deactivated or occupied and the last tile should be the one before the obstacle
                        range = r; // bad practice, setting an int within the function that's returning a type
                        targetTile.highlightColor = 2;
                        return targetTile;

                    }
                }
            }

        }
        // final target tile is highlighted with final highlight color
        targetTile.highlightColor = 2;

        return targetTile;
    }

    void UpdateScores()
    {
        for (int p = 0; p < players; p++)
        {
            if (p < 2)
            {
                scoreObjects[p].transform.GetComponent<Text>().text = "P" + (p + 1).ToString() + "\n" + playerList[p].GetPoints().ToString();
            }
            else
            {
                scoreObjects[p].transform.GetComponent<Text>().text = playerList[p].GetPoints().ToString() + "\nP" + (p + 1).ToString();
            }
        }
    }

    void MakeMove(player p, tile targetTile)
    {

        switchState(states.moving, 0.0f);
        // player makes a move

        // acquire points
        p.AddPoints(p.playerTile.points);

        // leave current tile
        liveHexGrid.leaveTile(p.playerTile);

        // move player unit to new tile
        // switch state to moving
        Vector3 sourcePos = p.playerGameObject.transform.position;
        Vector3 targetPos = new Vector3(targetTile.tileObject.transform.position.x, targetTile.tileObject.transform.position.y, p.playerGameObject.transform.position.z);
        // set player tile as the target tile
        p.playerTile = targetTile;
        activeTile = targetTile;
        liveHexGrid.enterTile(activeTile);

        // update scores
        UpdateScores();
        StartCoroutine(moveUnit(sourcePos, targetPos, p));




    }

    IEnumerator moveUnit(Vector3 sourcePos, Vector3 targetPos, player unit)
    {
        currentState = states.moving;
        Transform wheelChild = unit.playerWheelTransform;
        Vector2 lookPos = targetPos - sourcePos;
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, lookPos);
        float r = 0;
        float rStep = (1.0f / Vector3.Angle(lookPos, wheelChild.forward) * Time.fixedDeltaTime * 50);

        while (r < 0.5f)
        {
            r += rStep;
            wheelChild.rotation = Quaternion.Slerp(wheelChild.rotation, rotation, r);
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("Rotation complete, now moving");


        float step = (1.0f / (sourcePos - targetPos).magnitude * Time.fixedDeltaTime * 2);
        float t = 0;
        while (t < 1.0f)
        {
            t += step;
            unit.playerGameObject.transform.position = Vector3.Lerp(sourcePos, targetPos, t);
            yield return new WaitForFixedUpdate();
        }
        unit.playerGameObject.transform.position = targetPos;

        currentState = states.live;
        // Only once object has moved, do we increment to the next player
        yield return new WaitForSeconds(0.2f);
        IncrementActivePlayer();

    }

    void AIMove(player p)
    {
        tile targetTile;
        // see if a move is feasible
        if (ValidMoves(p))
        {
            if (currentState == states.live)
            {
                currentState = states.paused;
                switchState(states.moving, 0.2f);
                targetTile = PseudoAIMove(p);
                MakeMove(p, targetTile);
            }
        }

    }

    void CalculateTileMoveValues(player p)
    {
        // value weightings
        int firstMoveWeight = 4;
        int secondMoveWeight = 2;
        int thirdMoveWeight = 1;

        tile targetTile;
        List<tile> potentialTiles = new List<tile>();
        List<tile> potentialSecondTiles = new List<tile>();
        List<int> tileValues = new List<int>();

        int maxRange = hexGridRadius * 2;

        // first move valuation on the basis of the points on that tile
        // for every direction
        for (int d = 0; d < directions.Length; d++)
        {
            bool hitSnag = false;
            // for each tile in range

            for (int r = 1; r <= maxRange; r++)
            {
                if (!hitSnag)
                {
                    Vector3 relativeDir = directions[d] * r;

                    // check the tile by cycling all tiles
                    for (int i = 0; i < tileList.Count; i++)
                    {
                        // blank out mmove value for tiles
                        tileList[i].moveValues[activePlayer] = 0;
                        if (tileList[i].cubePosition == p.playerTile.cubePosition + relativeDir)
                        {
                            if (tileList[i].GetActive() && !tileList[i].GetOccupied())
                            {
                                potentialTiles.Add(tileList[i]);
                                // some weighting for guaranteed points from next move
                                tileValues.Add(tileList[i].points * firstMoveWeight);
                            }
                            else
                            {
                                // tile is blocked and the dir should be incremented 
                                hitSnag = true;
                            }
                        }
                    }
                }
            }
        }

        // First move valuation on the basis of the points reachable from the target tile
        for (int t = 0; t < potentialTiles.Count; t++)
        {
            // for each direction
            for (int d = 0; d < directions.Length; d++)
            {
                // for each tile in range
                for (int r = 1; r <= maxRange; r++)
                {
                    Vector3 relativeDir = directions[d] * r;

                    // check the tile by cycling all tiles
                    for (int i = 0; i < tileList.Count; i++)
                    {
                        if (tileList[i].cubePosition == potentialTiles[t].cubePosition + relativeDir)
                        {
                            if (tileList[i].GetActive() && !tileList[i].GetOccupied())
                            {
                                // some weighting for guaranteed points from next move
                                tileValues[t] += tileList[i].points * secondMoveWeight;
                            }
                        }
                    }
                }
            }
        }

        // at the end we should update all the tiles on the board with values and select the highest out of those (as long as it's a legit move)
    }

    tile PseudoAIMove(player p)
    {
        // value weightings
        int firstMoveWeight = 4;
        int secondMoveWeight = 1;
        int thirdMoveWeight = 1;

        tile targetTile;
        List<tile> potentialTiles = new List<tile>();
        List<tile> potentialSecondTiles = new List<tile>();

        List<int> tileValues = new List<int>();

        int maxRange = hexGridRadius * 2;
        // brute forcing it
        // unit contains own position?

        // What are the inputs and what should be the outputs?
        // needs: 
        // awareness of directions
        // awareness of maximum range
        // awareness of all tiles on the grid
        // awareness of the active player
        // weightings of the different points
        // output could be a value assigned to each tile

        // first move valuation on the basis of the points on that tile
        // for every direction
        for (int d = 0; d < directions.Length; d++)
        {
            bool hitSnag = false;
            // for each tile in range

            for (int r = 1; r <= maxRange; r++)
            {
                if (!hitSnag)
                {
                    Vector3 relativeDir = directions[d] * r;

                    // check the tile by cycling all tiles
                    for (int i = 0; i < tileList.Count; i++)
                    {
                        // blank out mmove value for tiles
                        tileList[i].moveValues[activePlayer] = 0;
                        if (tileList[i].cubePosition == p.playerTile.cubePosition + relativeDir)
                        {
                            if (tileList[i].GetActive() && !tileList[i].GetOccupied())
                            {
                                potentialTiles.Add(tileList[i]);
                                // some weighting for guaranteed points from next move
                                tileValues.Add(tileList[i].points * firstMoveWeight);
                            }
                            else
                            {
                                // tile is blocked and the dir should be incremented 
                                hitSnag = true;
                            }
                        }
                    }
                }
            }
        }

        // First move valuation on the basis of the points reachable from the target tile
        for (int t = 0; t < potentialTiles.Count; t++)
        {
            // for each direction
            for (int d = 0; d < directions.Length; d++)
            {
                // for each tile in range
                for (int r = 1; r <= maxRange; r++)
                {
                    Vector3 relativeDir = directions[d] * r;

                    // check the tile by cycling all tiles
                    for (int i = 0; i < tileList.Count; i++)
                    {
                        if (tileList[i].cubePosition == potentialTiles[t].cubePosition + relativeDir)
                        {
                            if (tileList[i].GetActive() && !tileList[i].GetOccupied())
                            {
                                // some weighting for guaranteed points from next move
                                tileValues[t] += tileList[i].points * secondMoveWeight;
                            }
                        }
                    }
                }
            }
        }

        // Think of the move +2
        // starting thinking of the move +1 
        // go through all promising tiles and add up diagonals' points

        // for each next potential tile
        for (int t = 0; t < potentialTiles.Count; t++)
        {

            potentialSecondTiles.Clear();
            // for each direction
            for (int d = 0; d < directions.Length; d++)
            {
                bool hitSnag = false;
                // for each tile in range from the new tile
                for (int r = 1; r <= maxRange; r++)
                {
                    if (!hitSnag)
                    {
                        Vector3 relativeDir = directions[d] * r;
                        // check the tile by cycling all tiles
                        for (int i = 0; i < tileList.Count; i++)
                        {
                            if (tileList[i].cubePosition == potentialTiles[t].cubePosition + relativeDir)
                            {
                                if (tileList[i].GetActive() && !tileList[i].GetOccupied())
                                {
                                    if (tileList[i].cubePosition != p.playerTile.cubePosition)
                                    {
                                        potentialSecondTiles.Add(tileList[i]);
                                        // tileValues.Add(tileList[i].points * secondMoveWeight);
                                    }
                                }
                                else
                                {
                                    // tile is blocked and the dir should be incremented 
                                    hitSnag = true;
                                }
                            }
                        }
                    }
                }
            }

            // For
        }

        // Think of blocking opponents immediate best move
        // for each opponent, do the same heuristic for the best immediate move, 
        // then if those tiles are within reach for this unit 
        // 
        // the AI heuristic for the above should somehow be more consolidated and rearchitectured
        // 

        // go through potential tiles 1 and check for diagonals of potential tiles 2 and add those values to the potential tile 1 move

        // then look at what's the likely first move of the opponent based on simple valuation and make those tiles more tempting


        List<tile> highValueTiles = new List<tile>();
        int highestValue = 0;
        for (int j = 0; j < tileValues.Count; j++)
        {
            if (tileValues[j] > highestValue)
            {
                highestValue = tileValues[j];
            }
        }
        for (int j = 0; j < tileValues.Count; j++)
        {
            if (tileValues[j] == highestValue)
            {
                highValueTiles.Add(potentialTiles[j]);
            }
        }

        // write move values in tiles
        for (int t = 0; t < potentialTiles.Count; t++)
        {
            potentialTiles[t].moveValues[activePlayer] = tileValues[t];
        }

        // finally decide at random from best valued tiles
        targetTile = highValueTiles[Random.Range(0, highValueTiles.Count)];
        Debug.Log("My name is Player" + p.playerNumber.ToString() + " and I'd like to move to tile " + targetTile.index.ToString());
        return targetTile;
    }



    public static class AI_Brain
    {


        /*
        List<tile> potentialTiles = new List<tile>();


        void EvaluateDiagonals(player p, int scoreWeighting)
        {
            for (int t = 0; t < potentialTiles.Count; t++)
            {
                // for each direction
                for (int d = 0; d < directions.Length; d++)
                {
                    // for each tile in range
                    for (int r = 1; r <= maxRange; r++)
                    {
                        Vector3 relativeDir = directions[d] * r;

                        // check the tile by cycling all tiles
                        for (int i = 0; i < tileList.Count; i++)
                        {
                            if (tileList[i].cubePosition == potentialTiles[t].cubePosition + relativeDir)
                            {
                                if (tileList[i].GetActive() && !tileList[i].GetOccupied())
                                {
                                    // some weighting for guaranteed points from next move
                                    tileValues[t] += tileList[i].points;
                                }
                            }
                        }
                    }
                }
            }

        }
        */

    }

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
