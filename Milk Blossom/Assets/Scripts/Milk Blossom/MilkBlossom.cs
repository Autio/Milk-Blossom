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
    private float turnCooldown = 0.5f;
    private float moveCoolDown = 2.0f;
    public float turnTimeLimit = 10f;
    private float turnTimeCounter = 0;
    public bool useAsInnerCircleRadius = true;
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
   
    void Awake()
    {
        // ART
        tileSprites = (Sprite[])Resources.LoadAll<Sprite>("Sprites\\HexSprite2");
    }

   

    public void SetPlayerDraggability()
    {
        foreach (player p in playerList)
        {
            if (p.playerNumber != (activePlayer + 1))
            {
                p.playerGameObject.transform.Find("PlayerSprite").GetComponent<TouchDrag>().enabled = false;
                p.playerGameObject.transform.Find("PlayerSprite").GetComponent<MouseDrag>().enabled = false;
            }
            else
            {
                if (p.GetAI())
                {
                    p.playerGameObject.transform.Find("PlayerSprite").GetComponent<TouchDrag>().enabled = false;
                    p.playerGameObject.transform.Find("PlayerSprite").GetComponent<MouseDrag>().enabled = false;
                }
                else
                {
                    p.playerGameObject.transform.Find("PlayerSprite").GetComponent<TouchDrag>().enabled = true;
                    p.playerGameObject.transform.Find("PlayerSprite").GetComponent<MouseDrag>().enabled = true;
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
                StartCoroutine(switchState(GameManager.states.ending, 2.0f));
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
        liveHexGrid.SetCoords(hexGridx, hexGridy);
        liveHexGrid.radius = hexRadius;
        liveHexGrid.useAsInnerCircleRadius = useAsInnerCircleRadius;
        liveHexGrid.playerCount = players;
        liveHexGrid.AIPlayerCount = AIPlayers;
        liveHexGrid.playerObj = playerObject;
        liveHexGrid.pointsObjects = pointsObjects;

        StartCoroutine(liveHexGrid.CreateHexShapedGrid(hexTile, hexGridRadius, GameManager.tileList, tileSprites, playerList));


        // once game is setup, set it to live
        StartCoroutine(switchState(GameManager.states.live, 5.0f));

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

    // Could be done with the state machine instead - 20170624
    IEnumerator switchState(GameManager.states s, float delay = 0.0f)
    {
        // first pause the game in transition limbo until the delay has passed
        GameManager.Instance.currentState = GameManager.states.paused;
        yield return new WaitForSeconds(delay);

        // then switch to desired state
        GameManager.Instance.currentState = s;

        if(GameManager.Instance.currentState == GameManager.states.live)
        {
            // Set active tile based on player
            activeTile = SelectPlayer(0);
            SetPlayerDraggability();

        }

        Debug.Log("Switced to " + s);

    }

    // Update is called once per frame
    void Update()
    {

        // Debug.Log("active player " + activePlayer);
        // Debug.Log("current state " + currentState);
        // RESET GAME
        if (Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        }

        if (GameManager.Instance.currentState == GameManager.states.live)
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
                liveHexGrid.DisplayMoveValues(activePlayer, GameManager.tileList);
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

        if (GameManager.Instance.currentState == GameManager.states.ending)
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
        foreach (tile t in GameManager.tileList)
        {
            t.SetHighlight(false, highlightColorList[0]);
        }
        try
        {
            playerList[activePlayer].playerTile.SetHighlight(true, highlightColorList[0]);
        }
        catch
        {

        }

    }

    void AllAllowedMovesHighlighter(tile sourceTile)
    {
        // Unhighlight all tiles
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
            Debug.Log("Could not highlight sourcetile");

        }
        try
        {
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
                                }
                                else
                                {
                                    t.SetHighlight(false, highlightColorList[0]);
                                    directionBlocked = true;

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
        } catch
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
        AllAllowedMovesHighlighter(activeTile);

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

        switchState(GameManager.states.moving, 0.0f);
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

    public bool MouseMakeMove(MoveablePiece m, int targetTileIndex)
    {
        // Tiles should always be referred to byt their index
        // This should return a boolean?
        
        // Feels like there should be a ValidMove parameter on the board tiles
        if(GameManager.tileList[targetTileIndex].GetHighlight())
        {
            IncrementActivePlayer();
            return true;
        } 
        else
        {
            return false;
        }

        // acquire points
        //p.AddPoints(p.playerTile.points);

        // leave current tile (by index)
        //liveHexGrid.leaveTile(p.playerTile);

        // arrive on new tile (by index)

        // set player tile as the target tile
        //p.playerTile = targetTile;
        //activeTile = targetTile;
        //liveHexGrid.enterTile(activeTile);

        // update scores
        UpdateScores();

        return false;

    }

    IEnumerator moveUnit(Vector3 sourcePos, Vector3 targetPos, player unit)
    {
        switchState(GameManager.states.moving, 0.0f);
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

        switchState(GameManager.states.live); ;
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
            if (GameManager.Instance.currentState == GameManager.states.live)
            {
                GameManager.Instance.currentState = GameManager.states.paused;
                switchState(GameManager.states.moving, 0.2f);
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
                    for (int i = 0; i < GameManager.tileList.Count; i++)
                    {
                        // blank out mmove value for tiles
                        GameManager.tileList[i].moveValues[activePlayer] = 0;
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
                        GameManager.tileList[i].moveValues[activePlayer] = 0;
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
