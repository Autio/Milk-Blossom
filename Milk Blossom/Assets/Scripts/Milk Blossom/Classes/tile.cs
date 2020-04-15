using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tile {

        public Vector3 cubePosition;
        public Vector2 offsetPosition;
        [Range(1, 3)]
        public int points;
        public int index;
        bool active = true;
        public bool occupied = false;
        bool validMove = false;
        bool highlighted = false;
        public int highlightColor;
        public GameObject tileObject;
        public int[] moveValues = new int[4]; // How valuable are the moves for players 1-4 
        public GameObject tilePointsObject;
        
        void drawPoints()
        {
            //
        }

        public void SetHighlight(bool isOn, Color highlightColor)
        {
            highlighted = isOn;
            if (highlighted)
            {
                tileObject.GetComponent<Renderer>().material.color = highlightColor;//highlightColorList[highlightColor];
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
        public void SetValidMove(bool isValid)
        {
            validMove = isValid;
        }
        public bool GetValidMove()
        {
            return validMove;
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
            // 

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
