using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hexGrid  {

    // what needs to be public here?
    int x = 5;
    int y = 5;
    public float radius = 0.5f;
    public bool useAsInnerCircleRadius = true;
    int tileCount = 0;
    public int playerCount = 2; // count, not index
    public int AIPlayerCount;
    public GameObject playerObj;
    public GameObject[] pointsObjects;

    private float offsetX, offsetY;
    private float standardDelay = 0.02f;
    private string superSecretMessage = "Videogames Rot Your Brains Videogames Rot Your Brains Videogames Rot Your Brains";
    // list of positions
    Vector3 maxBounds = new Vector3(0, 0, 0);
    Vector3 minBounds = new Vector3(0, 0, 0);
    public void SetCoords(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public IEnumerator CreateHexShapedGrid(GameObject hexTile, int gridRadius = 3, List<tile> tileList = null, Sprite[] tileSprites = null, List<player> playerList = null)
        {

            float delayModifier = 0.8f;
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

                GameObject newTileObject = (GameObject)UnityEngine.MonoBehaviour.Instantiate(hexTile, tileList[t].offsetPosition, Quaternion.identity);
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
               // tileList[t].tileObject.transform.Find("DebugText").gameObject.GetComponent<DebugTooltip>().debugText = superSecretMessage[t].ToString();
                yield return new WaitForSeconds(standardDelay);
            }

        // do remaining setup things within ienumerator to ensure sequence is correct
        // 2: Is this the correct place? 
            AllocatePoints(tileList);
            yield return new WaitForSeconds(0.8f);
            AllocatePlayers(playerObj, tileList, playerList);

            // Set starting player 
            // 2: This shouldn't always be the human player, should it?
            // activeTile = SelectPlayer(0); NEED TO SET ACTIVE TILE

        }

        // Place the points onto the tiles at the start of a level 
        public void AllocatePoints(List<tile> tileList = null)
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
                                float zOffset = 0.1f;
                                GameObject pointsObject = (GameObject)UnityEngine.MonoBehaviour.Instantiate(pointsObjects[k], new Vector3(chosenTile.tileObject.transform.position.x, chosenTile.tileObject.transform.position.y, zOffset), Quaternion.identity);
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
    public void AllocatePlayers(GameObject playerObject, List<tile> tileList = null, List<player> playerList = null)
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
                        GameObject newPlayer = (GameObject)UnityEngine.MonoBehaviour.Instantiate(playerObject, new Vector3(chosenTile.tileObject.transform.position.x, chosenTile.tileObject.transform.position.y, -0.5f), Quaternion.identity);
                        // set player text
                        newPlayer.transform.Find("PlayerSprite").transform.Find("PlayerLabel").GetComponent<TextMesh>().text = "P" + p.ToString();
                        chosenTile.SetOccupied(true);
                        player pl = new player();
                        playerList.Add(pl);
                        pl.playerNumber = p;
                        pl.playerTile = chosenTile;
                        pl.playerGameObject = newPlayer;
                        pl.playerWheelTransform = newPlayer.transform.Find("PlayerWheels");

                        if (playerCount - p < AIPlayerCount)
                        {
                            pl.SetAI(true);
                            Debug.Log("Player " + p.ToString() + " set to AI");
                        }


                        // first player always a human? Should be randomised
                        if (p == 1)
                        {
                            if (!pl.GetAI())
                            {

                                pl.playerGameObject.transform.Find("PlayerSprite").GetComponent<Drag>().enabled = true;
                            }
                        }

                        break;
                    }
                }
            }




        }

        public void CreateGrid(GameObject hexTile, List<tile> tileList = null)
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
                    GameObject newTileObject = (GameObject)UnityEngine.MonoBehaviour.Instantiate(hexTile, pos, Quaternion.identity);
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
        void AddDebugText(GameObject targetObject, string inputText, int fontSize = 18)
        {
            try
            {
                //string existingText = targetObject.transform.FindChild("debugtext").gameObject.GetComponent<DebugTooltip>().debugText;
                targetObject.transform.Find("DebugText").gameObject.GetComponent<DebugTooltip>().debugText = inputText;
            if (inputText == "")
            {
                targetObject.transform.Find("DebugText").Find("DebugTextBackground").gameObject.SetActive(false);
            } else
            {
                targetObject.transform.Find("DebugText").Find("DebugTextBackground").gameObject.SetActive(true);
                targetObject.transform.Find("DebugText").GetComponent<TextMesh>().fontSize = fontSize;
            }
        }
        catch
            {
                Debug.Log("Failed to add text");
            }

        }

        public void DisplayIndices(List<tile> tileList = null)
        {
            // simply number tiles
            for (int i = 0; i < tileList.Count; i++)
            {

                AddDebugText(tileList[i].tileObject, i.ToString());
            }
            if (tileList == null)
            {
                Debug.Log("DisplayIndices method being passed a blank tileList");
            }
        }
        public void DisplayCoords(List<tile> tileList = null)
        {
            // cubic coords
            for (int i = 0; i < tileList.Count; i++)
            {
                string coordText = tileList[i].cubePosition.x.ToString() + ", " + tileList[i].cubePosition.y.ToString() + ", " + tileList[i].cubePosition.z.ToString();
                AddDebugText(tileList[i].tileObject, coordText, 8);
            }
            if (tileList == null)
            {
                Debug.Log("DisplayCoords method being passed a blank tileList");
            }
    }
        public void DisplayPoints(List<tile> tileList = null)
        {
            // points
            for (int i = 0; i < tileList.Count; i++)
            {
                AddDebugText(tileList[i].tileObject, tileList[i].points.ToString());
            }
            if (tileList == null)
            {
                Debug.Log("DisplayPoints method being passed a blank tileList");
            }
    }

        public void DisplayMoveValues(int a, List<tile> tileList = null)
        {
            for (int i = 0; i < tileList.Count; i++)
            {
                AddDebugText(tileList[i].tileObject, tileList[i].moveValues[a].ToString());
        }
        if (tileList == null)
        {
            Debug.Log("DisplayMoveValues method being passed a blank tileList");
        }
    }

        public void DisplayClear(List<tile> tileList = null)
        {
            // clear all
            for (int i = 0; i < tileList.Count; i++)
            {
                AddDebugText(tileList[i].tileObject, "");
            }
            if (tileList == null)
            {
                Debug.Log("DisplayClear method being passed a null tileList");
            }
    }
    
    public int GetTileIndexByPos(Vector2 pos, List<tile> tileList)
    {
        foreach (tile t in tileList)
        {
            if (t.offsetPosition == pos)
            {
                return t.index;
            }
        }

        return -1;
    }

}
