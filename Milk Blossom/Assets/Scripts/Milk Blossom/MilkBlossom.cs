using UnityEngine;
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

        /* ---- To do 29/6/2017 ----
         * Proper handling of mouse movement -> make sure effects on grid are comprehensive
         * Make startup loop faster
         * Merge touch functionality to be parallel to mouse functionality
         * Test on phone
         * 
         * Map out separation of game logic
         * Finish game loop
         * Get unit art
         * Improve AI
         * Juice: Pop-up texts
         * Research / brainstorm juiciness
         * Think about audio: soundtrack & effects 
         */

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

    // Mouse control logic...
    // You should only be able to pick a legitimate piece to move around, i.e. your unit(s)
    // It should then identify which collider of a hex it's interacting with AT the point of the mouse drag being let go
    // It should then either glide back to the start position because it's an invalid move
    // Or make the move happen if it's a legitimate one

    // MAIN GAME LOGIC - MILK BLOSSOM
    Camera mainCam;
    bool firstTurn = true;
    Vector3[] directions = new Vector3[6];
    [Range(0, 5)]
    private int currentDir;

    public enum difficulty { easy, medium, hard }
    public difficulty diff = difficulty.easy;
        
    // Objects specific to Milk Blossom
    public GameObject[] pointsObjects;
    public GameObject hexTile;
    public GameObject[] scoreObjects;
    private GameObject timerBar;
    public bool timed = false; // do human players have limited time to do turns or not

    // Tile map attributes

    // Since tile maps should change per level this needs to be made less static
    public hexGrid liveHexGrid;
    public int hexGridx = 5;
    public int hexGridy = 5;
    public float hexRadius = 0.5f;
    public int hexGridRadius = 3;
    private int targetRange = 2;
    private float turnCooldown = 0.3f;
    private float moveCoolDown = 2.0f;
    public float turnTimeLimit = 10f;
    private float turnTimeCounter = 0;
    public bool useAsInnerCircleRadius = true;

    float gameLiveTime = 0.0f;

    static tile activeTile; // does it make sense to keep the active tile as a variable like this?

    // player info, would be better in a class probably. Limiting players to 4
    public GameObject playerObject;
    [Range(1, 4)]
    public int playerCount = 2;
    public int unitCount = 2;
    public int AIPlayerCount = 2;
    static List<player> playerList = new List<player>();
    public int activePlayerIndex = 0;
    public int activeUnitIndex = 0;

    // ART & VISUAL
    static Sprite[] tileSprites;
    public static Color[] highlightColorList = new Color[3]; // 0 for source, 1 for midway and 2 for target
    public Color[] highlightColorsListPublic;
    public GameObject endText;

    // LOAD RESOURCES
    void Awake()
    {
        // ART
        tileSprites = (Sprite[])Resources.LoadAll<Sprite>("Sprites\\HexSprite2");
    }

    // MAIN GAME FLOW - START ->
    void Start()
    {
        // DEFAULT TO ENGLISH - add menu options for other languages. Looks like the folder needs to be different if running on Android. 
        LocalisationManager.Instance.LoadLocalisedText("English.json");
        Debug.Log(LocalisationManager.Instance.GetLocalisedValue("game_title"));

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

    void InitGame()
    {
        // create grid class, allocate points and allocate players
        liveHexGrid = new hexGrid();
        liveHexGrid.SetCoords(hexGridx, hexGridy);
        liveHexGrid.radius = hexRadius;
        liveHexGrid.useAsInnerCircleRadius = useAsInnerCircleRadius;
        liveHexGrid.playerCount = playerCount;
        liveHexGrid.AIPlayerCount = AIPlayerCount;
        liveHexGrid.unitCount = unitCount;
        liveHexGrid.playerObj = playerObject;
        liveHexGrid.pointsObjects = pointsObjects;

        // Instantiate grid and spawn players to the side of the board
        StartCoroutine(liveHexGrid.CreateHexShapedGrid(hexTile, hexGridRadius, GameManager.tileList, tileSprites, playerList));

        // once game is setup, the placement phase begins
        StartCoroutine(switchState(GameManager.states.placing, 3.4f));

        // set player amounts
        for (int i = 0; i < playerCount; i++)
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

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("active player " + activePlayer);
        // Debug.Log("current state " + GameManager.Instance.currentState);
        // RESET GAME
        if (Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        if(Input.GetKey(KeyCode.T))
        {
            Toolbox.Instance.SpawnText("HELLO WORLD", new Vector3(0,0, -4.0f), 2.5f, 0.5f, 0.1f);
            StartCoroutine(Pause(0.5f, GameManager.states.live));
        }
        if (GameManager.Instance.currentState == GameManager.states.placing)
        {
            Placing();
        }
        
        if (GameManager.Instance.currentState == GameManager.states.live)
        {
            Live();
        }
        if (GameManager.Instance.currentState == GameManager.states.ending)
        {
            Ending();
        }
    }

    void Placing()
    { 

        // Allow placement of units by dragging from the side onto single piece tiles
        // In player order: 1st, 2nd, 3rd etc then back to 1st
        // Set valid placement tiles on each update cycle

        player activePlayer = null;
        // If the active player is a human, then do human placement
        if (!playerList[activePlayerIndex].GetAI())
        {
            AllAllowedPlacements();

            // Allow draggability of the correct playerunit only
            // unit index should increment when last player flips to first one
            // The dragging itself happens with the Drag class
            SetPlayerPlacementDraggability(activeUnitIndex + 1);
            // MakePlacement();
        }
        // If the active player is an AI, do AI placement
        else
       {
            // TESTING AI PLACEMENT
            activePlayer = playerList[activePlayerIndex];
            AIPlace(activePlayer, 1.0f);    
        }
        // Check what should happen next:
        // Player places a unit. If the unit is placed, it no longer can be dragged in placement! 
        // Only the remaining unit(s) on the sidelines can be placed      
    }

    void Live()
    {
        gameLiveTime += Time.deltaTime;
        // Main game loop
        // One player at a time selects one of their units from the board and makes a legitimate move
        // Points are counted from beneath

        // ClearHighlights();
        // Only highlight player unit tiles when the unit isn't being dragged
        // Don't do this every cycle

           // HighlightPlayerUnitTiles(activePlayerIndex);
        

        if(playerList[activePlayerIndex].GetAI())
        {
            // make AI move
            AIMove(diff);
        }
        /*
        turnCooldown -= Time.deltaTime;
        if (turnCooldown < 0)
        {
            // check if it's AI's turn to go
            if (playerList[activePlayerIndex].GetAI())
            {
                turnCooldown = 0.75f;              IncrementActivePlayer();
                AIMove(playerList[activePlayerIndex]);

            }
            else
            {
                turnCooldown = 0.03f;
            }
        }
        */

        /*if(firstTurn)
        {
            // Should highlight the units of the active player's units
            // And when that unit is picked up, all the valid move tiles should be highlighted
            HighlightPlayerUnitTiles(activePlayerIndex);
            AllAllowedMoves(activeTile);
            firstTurn = false;
        }*/

        // TURN TIMER
        // only count for human players
        if (timed)
        {
            if (!playerList[activePlayerIndex].GetAI())
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
    }

    void Ending()
    {
        // Include the final tiles for each player here
        // Grab high score
        int hiPlayer = 0;
        int hiScore = 0;
        for (int i = 0; i < playerCount; i++)
        {
            if (playerList[i].GetPoints() > hiScore)
            {
                hiPlayer = i + 1;
                hiScore = playerList[i].GetPoints();
            }

        }
        endText.transform.GetComponent<Text>().text = "ENDED\nPlayer " + hiPlayer.ToString() + "\nWins";
    }

    // MAIN GAME FLOW - END
    // PLACEMENT AND MOVEMENT FUNCTIONS
    public void SetPlayerDraggability()
    {
        foreach (player p in playerList)
        {
            if (p.playerNumber != (activePlayerIndex + 1)) // +1 is correct
            {
                //   p.playerGameObject.transform.Find("PlayerSprite").GetComponent<TouchDrag>().enabled = false;
                p.playerGameObject.transform.Find("PlayerSprite").GetComponent<Drag>().enabled = false;
            }
            else
            {
                if (p.GetAI())
                {
                    //     p.playerGameObject.transform.Find("PlayerSprite").GetComponent<TouchDrag>().enabled = false;
                    p.playerGameObject.transform.Find("PlayerSprite").GetComponent<Drag>().enabled = false;
                }
                else
                {
                    //   p.playerGameObject.transform.Find("PlayerSprite").GetComponent<TouchDrag>().enabled = true;
                    p.playerGameObject.transform.Find("PlayerSprite").GetComponent<Drag>().enabled = true;
                }
            }
        }
    }

    void ClearPlayerPlacementDraggability()
    {
        foreach (player p in playerList)
        {
            p.playerGameObject.transform.Find("PlayerSprite").GetComponent<Drag>().enabled = false;
            p.playerGameObject.transform.Find("PlayerSprite").GetComponent<Renderer>().material.color = Color.grey;
        }
    }

    void SetPlayerPlacementDraggability(int unitNumber)
    {
        foreach (player p in playerList)
        {
            if (p.playerNumber != ((activePlayerIndex) % playerCount) + 1) // +1 is correct
            {
                //   p.playerGameObject.transform.Find("PlayerSprite").GetComponent<TouchDrag>().enabled = false;
                p.playerGameObject.transform.Find("PlayerSprite").GetComponent<Drag>().enabled = false;
                p.playerGameObject.transform.Find("PlayerSprite").GetComponent<Renderer>().material.color = Color.grey;

            }
            else
            {
                if (p.GetAI())
                {
                    //     p.playerGameObject.transform.Find("PlayerSprite").GetComponent<TouchDrag>().enabled = false;
                    p.playerGameObject.transform.Find("PlayerSprite").GetComponent<Drag>().enabled = false;
                }
                else
                {
                    if (p.unitNumber == unitNumber)
                    {
                        //   p.playerGameObject.transform.Find("PlayerSprite").GetComponent<TouchDrag>().enabled = true;
                        p.playerGameObject.transform.Find("PlayerSprite").GetComponent<Drag>().enabled = true;
                        // Highlight the sprite
                        p.playerGameObject.transform.Find("PlayerSprite").GetComponent<Renderer>().material.color = Color.blue;
                    }
                    else

                    {
                        p.playerGameObject.transform.Find("PlayerSprite").GetComponent<Drag>().enabled = false;
                        p.playerGameObject.transform.Find("PlayerSprite").GetComponent<Renderer>().material.color = Color.grey;
                    }
                }
            }
        }
    }

    // Player Incrementation
    void IncrementActivePlayer()
    {
        activePlayerIndex++;
        // Cycle back to the start 
        if (activePlayerIndex >= playerCount)
        {
            activePlayerIndex = 0;
        }

        // Only the current player can be dragged and dropped, if it's not an AI
        SetPlayerDraggability();

        // Show all possible moves when dragging & dropping
        ClearHighlights();
        HighlightPlayerUnitTiles(activePlayerIndex);

        // Go through all players
        foreach(player p in playerList)
        {
            // If the unit belongs to the active player 
            if(p.playerNumber == (activePlayerIndex + 1))
            {
                // See if the unit has any valid moves left, if not then disable that unit
                if(!ValidMoves(p))
                {
                    
                    Debug.Log("No valid moves for player " + (activePlayerIndex + 1).ToString() + " unit " + p.unitNumber.ToString()); // +1 correct
                    if (p.GetAlive())
                    {
                        // can't make moves
                        // player UNIT is taken out of circulation
                        p.SetAlive(false);
                        p.DeathThroes();

                        if (CheckPlayersAlive())
                        {
                            IncrementActivePlayer();
                        }
                        else
                        {
                            // transition to end state
                            StartCoroutine(switchState(GameManager.states.ending, 2.0f));
                        }
                    }
                }


            }
            else
            {
                Debug.Log("All fine");
            }
        }

    }

    static tile SelectPlayer(int playerNumber = 0)
    {
        Debug.Log("player number " + (playerNumber + 1).ToString() + " selected");
        foreach (player p in playerList)
        {
            Debug.Log("Selecting player " + p.playerNumber + " unit " + p.unitNumber);
            if (p.playerNumber == (playerNumber))
            {
                Debug.Log("Setting active tile");
                return p.playerTile;
            }
        }
        return null;
    }
    
    void IncrementPlacementPlayer()
    {
        activePlayerIndex++;
        if (activePlayerIndex % playerCount == 0)
        {
            activeUnitIndex += 1;

        }
        ClearPlayerPlacementDraggability();
        ClearHighlights();

        if (activePlayerIndex >= (playerCount * unitCount))
        {
            Debug.Log("Active unit index reached unit count maximum. Switching to live.");
            // This should end the placement phase
            activePlayerIndex = 0;
            activeUnitIndex = 0;
            StartCoroutine(switchState(GameManager.states.live, 0.0f));
        }
    }

    public bool CheckMove(int targetTileIndex)
    {
        // This is dependent on the board valid moves being updated appropriately for the active player
        tile targetTile = GameManager.tileList[targetTileIndex];
        if (GameManager.tileList[targetTileIndex].GetValidMove()) // should be "IsValid"
        {
            return true;
        }
        else
        {
            return false;
        }
                   
    }
    public bool CheckPlacement(int targetTileIndex)
    {
        // This is dependent on the board valid moves being updated appropriately for the active player
        tile targetTile = GameManager.tileList[targetTileIndex];
        if (GameManager.tileList[targetTileIndex].GetValidMove()) // should be "IsValid"
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    public void MakePlacement(int targetTileIndex = 0, GameObject playerGameObject = null)
    {
        string s = "";
        // Just make sure the correct playerUnitIndex is passed through
        player placementPlayer = null;
        float incr = 0.2f;
        foreach (player pl in playerList)
        {

            if (pl.playerGameObject == playerGameObject)
            {
                placementPlayer = pl;
                s = pl.playerNumber.ToString() + "_" + pl.unitNumber.ToString();
                Debug.Log("Player unit being placed " + s);

            }
        }

        Debug.Log("Placing " + s + " to tile index " + targetTileIndex.ToString());
        liveHexGrid.enterTile(GameManager.tileList[targetTileIndex]);
        placementPlayer.playerTile = GameManager.tileList[targetTileIndex];

        ClearHighlights();

        // Each player places one unit and then it loops around to the first until all are placed
        IncrementPlacementPlayer();
    }
    private IEnumerator Place_Routine(Transform transform, Vector3 from, Vector3 to, float duration = 1.0f, GameManager.states s = GameManager.states.paused)
    {
        float t = 0f;
        yield return new WaitForSeconds(0.2f);
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // Switch back to live only after the move is completed
        // GameManager.Instance.currentState = s;
    }

    private IEnumerator Move_Routine(Transform transform, Vector3 from, Vector3 to, float duration = 1.0f, GameManager.states s = GameManager.states.paused)
    {
        float t = 0f;
        yield return new WaitForSeconds(0.2f);
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // Switch back to live only after the move is completed
        GameManager.Instance.currentState = s;
    }


    public void MakeMove(int sourceTileIndex = 0, int targetTileIndex = 0, int playerIndex = 0)
    {
        player p = null;
        if(playerIndex == 0)
        {
            for(int i = 0; i < playerList.Count; i++)
            {

            
                if(GameManager.tileList[sourceTileIndex] == playerList[i].playerTile)
                {
                        p = playerList[i];
                }

            }
            
        }
     

        // update scores
        UpdateScores();

        liveHexGrid.leaveTile(GameManager.tileList[sourceTileIndex]);
        activeTile = GameManager.tileList[targetTileIndex];
        liveHexGrid.enterTile(activeTile);
        p.playerTile = GameManager.tileList[targetTileIndex];
        Debug.Log("Setting player " + p.playerNumber.ToString() + " unit " + p.unitNumber.ToString() + " to tile of index " + targetTileIndex.ToString());

        // arrive on new tile (by index)
        IncrementActivePlayer();

        // Acquire points - need
        // 1) point amount, so need the tile the player is moving away from
        // 2) Display a popup text with that value and that should happen from the source tile
        AcquirePoints(GameManager.tileList[sourceTileIndex], p);


    }
    
    IEnumerator AIPlacementRoutine(player p, int tileIndex, float delay)
    {

        tile t = GameManager.tileList[tileIndex];
        Debug.Log("AI placing player " + p.playerNumber.ToString() + " unit " + p.unitNumber.ToString());
        StartCoroutine(Place_Routine(p.playerGameObject.transform, p.playerGameObject.transform.position, new Vector3(t.offsetPosition.x, t.offsetPosition.y, 
            p.playerGameObject.transform.position.z), 2.5f, GameManager.states.placing));
        yield return new WaitForSeconds(delay);
        liveHexGrid.enterTile(GameManager.tileList[tileIndex]);
        p.playerTile = GameManager.tileList[tileIndex];
        IncrementPlacementPlayer();
        GameManager.Instance.currentState = GameManager.states.placing;

    }

    IEnumerator moveUnit(Vector3 sourcePos, Vector3 targetPos, player unit)
    {
        StartCoroutine(switchState(GameManager.states.moving, 0.0f));
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

        StartCoroutine(switchState(GameManager.states.live));
        // Only once object has moved, do we increment to the next player
        yield return new WaitForSeconds(0.2f);
        IncrementActivePlayer();

    }

    void KeyboardControls()
    {
        // CONTROLS
        if (Input.GetKey(KeyCode.H))
        {
            HighlightPlayerUnitTiles(activePlayerIndex);
        }


        // debug visualisations 
        if (Input.GetKey(KeyCode.F1))
        {
            liveHexGrid.DisplayIndices(GameManager.tileList);
        }
        if (Input.GetKey(KeyCode.F2))
        {
            liveHexGrid.DisplayCoords(GameManager.tileList);
        }
        if (Input.GetKey(KeyCode.F3))
        {
            liveHexGrid.DisplayPoints(GameManager.tileList);
        }

        if (Input.GetKey(KeyCode.F4))
        {
            liveHexGrid.DisplayMoveValues(activePlayerIndex, GameManager.tileList);
        }
        if (Input.GetKey(KeyCode.F5))
        {
            liveHexGrid.DisplayClear(GameManager.tileList);
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

        /*
        if (GameManager.Instance.activeControlOptions.(GameManager.controlOptions.mouse))
        {

        }
        */

        tile targetTile = null;

        if (Input.GetKey(KeyCode.Return))
        {
            if (activeTile != targetTile)
            {

             //   MakeMove(playerList[activePlayerIndex], targetTile);
                ClearHighlights();

                // IncrementativePlayer();
            }
        }

        if (Input.GetKey(KeyCode.T))
        {
            PseudoAIMove(playerList[activePlayerIndex]);
        }
    }

    // ------- AI ------- //

    void AIPlace(player p, float delay = 1.0f)
    {
        // Refresh board valuation
        EvaluateTileValues(true);

        int maxValue = 0;
        int targetTileIndex = 0;
        // Choose the optimal tile or thereabouts
        foreach (tile t in GameManager.tileList)
        {
            if (t.GetValidMove())
            {
                if (t.moveValues[0] > maxValue)
                {
                    maxValue = t.moveValues[0];
                    targetTileIndex = t.index;
                }
            }
      
        }
        if (maxValue == 0)
        {
            Debug.Log("No moves available!");
        }
        p.playerTile = GameManager.tileList[targetTileIndex];

        // Set the game to be in an intermediate state so that we have to wait until the move is completed before making he next placement 

        // Place the player unit onto the target tile once it's been selected
        GameManager.Instance.currentState = GameManager.states.moving;
        StartCoroutine(AIPlacementRoutine(p, targetTileIndex, delay));

    }

    void AIMove(difficulty d)
    {
        // Go through all units of the player and then decide which one to move and where
        int preferredUnit = 0;
        int maxValue = 0;
        int targetTileIndex = 0;
        player p = null;

        if (d == difficulty.easy)
        {
            foreach (player pl in playerList)
            {
                if (pl.playerNumber == (activePlayerIndex + 1))
                {
                    // This unit belongs to the active player
                    // Evaluate all possible tiles
                    EvaluateTileValues(false);

                    // Set allowed moves based on unit location
                    AllAllowedMoves(pl.playerTile);

                    // Choose the optimal tile or thereabouts
                    foreach (tile t in GameManager.tileList)
                    {
                        // is the move valid? 
                        if (t.GetValidMove())
                        {
                            if (t.moveValues[0] > maxValue)
                            {
                                maxValue = t.moveValues[0];
                                targetTileIndex = t.index;

                                preferredUnit = pl.unitNumber;
                            }
                        }
                    }
                }
            }

            // Decide between units
            // Basic decision: highest points value always wins
            foreach (player pl in playerList)
            {
                if (pl.unitNumber == preferredUnit && pl.playerNumber == activePlayerIndex + 1)
                {
                    p = pl;
                }
            }

        }

        // Leave previous tile
        liveHexGrid.leaveTile(p.playerTile);
        AcquirePoints(p.playerTile, p);

        // Enter next one
        liveHexGrid.enterTile(GameManager.tileList[targetTileIndex]);
        p.playerTile = GameManager.tileList[targetTileIndex];
        tile tl = p.playerTile;

        GameManager.Instance.currentState = GameManager.states.moving;

        StartCoroutine(Move_Routine(p.playerGameObject.transform, p.playerGameObject.transform.position, new Vector3(tl.offsetPosition.x, tl.offsetPosition.y, p.playerGameObject.transform.position.z),
            2.5f, GameManager.states.live));

        IncrementActivePlayer();

        /*
        // see if a move is feasible
        if (ValidMoves(p))
        {
            if (GameManager.Instance.currentState == GameManager.states.live)
            {
                GameManager.Instance.currentState = GameManager.states.paused;
                switchState(GameManager.states.moving, 0.2f);
                targetTile = PseudoAIMove(p);
             //   MakeMove(p, targetTile);
            }
        }*/

    }

    // EVALUATING THE BOARD
    // MOVE VALUE CALCULATION

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
                    for (int i = 0; i < GameManager.tileList.Count; i++)
                    {
                        // blank out mmove value for tiles
                        GameManager.tileList[i].moveValues[activePlayerIndex] = 0;
                        if (GameManager.tileList[i].cubePosition == p.playerTile.cubePosition + relativeDir)
                        {
                            if (GameManager.tileList[i].GetActive() && !GameManager.tileList[i].GetOccupied())
                            {
                                potentialTiles.Add(GameManager.tileList[i]);
                                // some weighting for guaranteed points from next move
                                tileValues.Add(GameManager.tileList[i].points * firstMoveWeight);
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
                    for (int i = 0; i < GameManager.tileList.Count; i++)
                    {
                        if (GameManager.tileList[i].cubePosition == potentialTiles[t].cubePosition + relativeDir)
                        {
                            if (GameManager.tileList[i].GetActive() && !GameManager.tileList[i].GetOccupied())
                            {
                                // some weighting for guaranteed points from next move
                                tileValues[t] += GameManager.tileList[i].points * secondMoveWeight;
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
                    for (int i = 0; i < GameManager.tileList.Count; i++)
                    {
                        // blank out mmove value for tiles
                        GameManager.tileList[i].moveValues[activePlayerIndex] = 0;
                        if (GameManager.tileList[i].cubePosition == p.playerTile.cubePosition + relativeDir)
                        {
                            if (GameManager.tileList[i].GetActive() && !GameManager.tileList[i].GetOccupied())
                            {
                                potentialTiles.Add(GameManager.tileList[i]);
                                // some weighting for guaranteed points from next move
                                tileValues.Add(GameManager.tileList[i].points * firstMoveWeight);
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
                    for (int i = 0; i < GameManager.tileList.Count; i++)
                    {
                        if (GameManager.tileList[i].cubePosition == potentialTiles[t].cubePosition + relativeDir)
                        {
                            if (GameManager.tileList[i].GetActive() && !GameManager.tileList[i].GetOccupied())
                            {
                                // some weighting for guaranteed points from next move
                                tileValues[t] += GameManager.tileList[i].points * secondMoveWeight;
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
                        for (int i = 0; i < GameManager.tileList.Count; i++)
                        {
                            if (GameManager.tileList[i].cubePosition == potentialTiles[t].cubePosition + relativeDir)
                            {
                                if (GameManager.tileList[i].GetActive() && !GameManager.tileList[i].GetOccupied())
                                {
                                    if (GameManager.tileList[i].cubePosition != p.playerTile.cubePosition)
                                    {
                                        potentialSecondTiles.Add(GameManager.tileList[i]);
                                        // tileValues.Add(GameManager.tileList[i].points * secondMoveWeight);
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
            potentialTiles[t].moveValues[activePlayerIndex] = tileValues[t];
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

    ///  SCORE FUNCTIONS    
    void AcquirePoints(tile t, player p)
    {    
        // Display that increase as a floating text above the tile that was left
        Toolbox.Instance.SpawnText(t.points.ToString() + LocalisationManager.Instance.GetLocalisedValue("points"), new Vector3(t.offsetPosition.x, t.offsetPosition.y, -4.0f), 1.5f, 0.5f, 0.1f);
        // Increment the point count of the active player
        p.AddPoints(t.points);
        StartCoroutine(Pause(0.5f, GameManager.states.live));

    
    }
    void UpdateScores()
    {
        for (int p = 0; p < playerCount; p++)
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


    // -----------------------------------
    // HIGHLIGHTING and MOVE VALIDITY FUNCTIONS
    // Highlight in a line in one direction
    public void ClearHighlights()
    {
        foreach (tile t in GameManager.tileList)
        {
            t.SetHighlight(false, highlightColorList[0]);
        }
    }

    void HighlightPlayerUnitTiles(int playerIndex)
    {
//        Debug.Log("Highlighting player unit tiles");
        foreach (tile t in GameManager.tileList)
        {
            //t.SetHighlight(false, highlightColorList[0]);
            foreach (player p in playerList)
            {
                if (p.playerNumber == (activePlayerIndex + 1))
                {
                    try
                    {
                        p.playerTile.SetHighlight(true, highlightColorList[1]);
                    }
                    catch
                    {
                        Debug.Log("Could not set highlight for player " + p.playerNumber.ToString() + " unit " + p.unitNumber.ToString());

                    }
                }
                else
                {
                    try
                    {
                        p.playerTile.SetHighlight(false, highlightColorList[0]);
                    }
                    catch
                    {
                        Debug.Log("Could not set highlight for player " + p.playerNumber.ToString() + " unit " + p.unitNumber.ToString());
                    }
                }
            }
        }
    }

    void AllAllowedPlacements()
    {
        foreach (tile t in GameManager.tileList)
        {
            t.SetHighlight(false, highlightColorList[0]);
            if (t.GetOccupied() == false && t.points == 1)
            {
                t.SetHighlight(true, highlightColorList[1]);
                t.SetValidMove(true);
            }
            else
            {
                t.SetValidMove(false);
            }
        }
    }


    // if switching to player, check if any valid moves are available
    public bool ValidMoves(player p)
    {
        for (int d = 0; d < directions.Length; d++)
        {
            foreach (tile t in GameManager.tileList)
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
   

    public void AllAllowedMoves(tile sourceTile)
    {
        // Unhighlight all tiles and set them as invalid moves
        foreach (tile t in GameManager.tileList)
        {
            t.SetHighlight(false, highlightColorList[0]);
            t.SetValidMove(false);
        }
        try
        {
            sourceTile.SetHighlight(true, highlightColorList[1]);
        }
        catch
        {
            Debug.Log("Could not highlight sourcetile");

        }
        try
        {
            Debug.Log("Highlighting directions");
            for (int d = 0; d < 6; d++)
            {
                bool directionBlocked = false;
                for (int r = 1; r <= (hexGridRadius * 2); r++)
                {

                    Vector3 relativeTargetPosition = directions[d] * r;
                    // try and step to the next tile in the direction
                    // does the tile exist
                    foreach (tile t in GameManager.tileList)
                    {
                        if (t.cubePosition == sourceTile.cubePosition + relativeTargetPosition)
                        {
                            if (!directionBlocked)
                            {
                                if (t.GetActive() && !t.GetOccupied())
                                {
                                    t.SetHighlight(true, highlightColorList[0]);
                                    t.SetValidMove(true);
                                }
                                else
                                {
                                    t.SetHighlight(false, highlightColorList[0]);
                                    directionBlocked = true;
                                    t.SetValidMove(false);
                                    // if it's not a valid tile then it is either deactivated or occupied and the last tile should be the one before the obstacle
                                    //range = r; // bad practice, setting an int within the function that's returning a type
                                    //targetTile.highlightColor = 2;
                                    //return targetTile;

                                }
                            }
                            else
                            {
                                t.SetHighlight(false, highlightColorList[0]);
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            Debug.Log("Unable to highlight");
        }
    }
    tile LinearHighlighter(tile sourceTile, int direction, int range)
    {
        // The 5 is linked to the board size
        Mathf.Clamp((float)direction, 0, 5);

        // there should be a way to know from the cubic coordinates whether the tile is on a line
        // Refresh the highlighted tiles for all in range
        AllAllowedMoves(activeTile);

        // first, unhighlight all tiles
        foreach (tile t in GameManager.tileList)
        {
            t.SetHighlight(false, highlightColorList[0]);
        }

        try
        {
            sourceTile.SetHighlight(true, highlightColorList[0]);
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
            try
            {
                foreach (tile t in GameManager.tileList)
                {
                    if (t.cubePosition == sourceTile.cubePosition + relativeTargetPosition)
                    {
                        if (t.GetActive() && !t.GetOccupied())
                        {
                            t.SetHighlight(true, highlightColorList[0]);
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
            catch
            {
                Debug.Log("Error with tile highlighting");
            }

        }
        // final target tile is highlighted with final highlight color
        targetTile.highlightColor = 2;

        return targetTile;
    }

  


    // -----------------------------------
    // HELPER FUNCTIONS
    private bool CheckPlayersAlive()
    {
        // As long as there's a single player left alive, return true
        for (int p = 0; p < playerCount; p++)
        {
            if (playerList[p].GetAlive())
            {
                return true;
            }
        }
        return false;
    }
    // Could be done with the state machine instead - 20170624
    IEnumerator switchState(GameManager.states s, float delay = 0.0f)
    {
        // first pause the game in transition limbo until the delay has passed
        GameManager.Instance.currentState = GameManager.states.paused;
        yield return new WaitForSeconds(delay);

        // then switch to desired state
        GameManager.Instance.currentState = s;

        if (GameManager.Instance.currentState == GameManager.states.live)
        {
            // Set active tile based on player
            // Need to update to take into acconut multi-units
            activeTile = SelectPlayer(activePlayerIndex);
            SetPlayerDraggability();

        }

        Debug.Log("Switched to " + s);
        // Display current state on top text if it's active
        try
        {
            GameObject.Find("TopNotice").GetComponent<Text>().text = s.ToString();
        }
        catch
        {
            Debug.Log("");
        }
    }


    IEnumerator Wait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        AllAllowedMoves(activeTile);
    }

    // Algorithm for placing AI units
    // Assign a value to all tiles
    void EvaluateTileValues(bool place = false)
    {
        // FirstLevelValue = Sum of all point values accessible from the tile
        int moveVal = 0;



        // Go through all tiles 
        foreach (tile t in GameManager.tileList)
        {
            moveVal = 0;

            t.moveValues[0] = moveVal;

            // Only evaluate if the tile isn't occupied - if it's occupied it can't be reached anyway
            if (!t.GetOccupied())
            {           
                // If not placing, also count the tile value itself 

                if (!place)
                {
                    moveVal += t.points;
                }

                // Check each direction
                for (int i = 0; i < directions.Length; i++)
                {
                    bool dirBlocked = false;
                    for (int r = 1; r <= (hexGridRadius * 2); r++) // works for hex shaped board
                    {
                        Vector3 relPosition = directions[i] * r;
                        foreach (tile t2 in GameManager.tileList)
                        {
                            if (t2.cubePosition == t.cubePosition + relPosition)
                            {
                                if (t2.GetActive() && !t2.GetOccupied())
                                {
                                    if (t2 != t)
                                    {
                                        moveVal += t2.points;
                                    }
                                }
                                else
                                {
                                    // if the direction is blocked, stop looking that way
                                    dirBlocked = true;
                                    break;
                                }
                            }

                        }
                        if (dirBlocked)
                        {
                            break;
                        }
                    }
                    t.moveValues[0] = moveVal;
                    Debug.Log("Tile " + t.index.ToString() + " move value is " + t.moveValues[0].ToString());
                }
            }

        }// 
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

    IEnumerator Move(float duration, GameManager.states returnState)
    {
        GameManager.Instance.currentState = GameManager.states.moving;
        yield return new WaitForSeconds(duration);
        GameManager.Instance.currentState = returnState;

    }

    IEnumerator Pause(float duration, GameManager.states returnState)
    {
        GameManager.Instance.currentState = GameManager.states.paused;
        yield return new WaitForSeconds(duration);
        GameManager.Instance.currentState = returnState;

    }
}
