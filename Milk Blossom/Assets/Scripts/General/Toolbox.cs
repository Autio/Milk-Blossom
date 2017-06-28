using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbox : Singleton<Toolbox> {
// General place for global variables
    protected Toolbox() { } // guarantees this will only be a singleton because you can't use a constructor

    public string testVar = "What";

    // Create a floating text on the screen which disappears once its lifetime is over
    public void SpawnText(string textString, Vector3 pos, float lifetime = 2.0f, float textSpeed = 0.6f, float startDelay = 0.2f)
    {
        StartCoroutine(SpawnTextTimed(textString, pos, lifetime, textSpeed, startDelay));
    }

    IEnumerator SpawnTextTimed(string textString, Vector3 pos, float lifetime = 2.0f, float textSpeed = 0.6f, float startDelay = 0.2f)
    {
        yield return new WaitForSeconds(startDelay);
        GameObject newText = new GameObject();
        newText.transform.parent = this.transform;
        newText.transform.position = pos;
        TextMesh tm = newText.gameObject.AddComponent<TextMesh>();
        tm.text = textString;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.fontSize = 50;
        tm.characterSize = 0.1f;

        Destroy(newText, lifetime);
        StartCoroutine(MoveOverTime(newText.transform, textSpeed, lifetime));

    }

    public IEnumerator MoveOverTime(Transform t, float speed, float lifetime)
    {

        while (true)
        {
            yield return Time.deltaTime;
            lifetime -= Time.deltaTime;

            t.position = t.position + (Vector3.up * speed * Time.deltaTime);
            // Break loop before the object itself gets destroyed
            if (lifetime < 0.1f)
            {
                break;

            }
        }
    }


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
