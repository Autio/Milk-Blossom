using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class LocalisationManager :  Singleton<LocalisationManager> {

    protected LocalisationManager() { }

    private Dictionary<string, string> localisedText;
    private bool isReady = false;
    private string missingTextString = "Localised text not found";




    public void LoadLocalisedText(string fileName)
    {
        localisedText = new Dictionary<string, string>();
        string filePath = "Localisation/" + fileName.Replace(".json", "");
        TextAsset targetFile = Resources.Load<TextAsset>(filePath);
        string tF = targetFile.text;
        // Resources folder 
       // string fileData = Resources.Load<TextAsset>(targetFile).text;
        // Streaming assets

        //filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        try {
            //(File.Exists(filePath + ".json"))
            
            //string dataAsJSON = File.ReadAllText(filePath);
            LocalisationData loadedData = JsonUtility.FromJson<LocalisationData>(tF);

            for (int i = 0; i < loadedData.items.Length; i++)
            {
                localisedText.Add(loadedData.items[i].key, loadedData.items[i].value);
            }
            Debug.Log("Data loaded, dictionary contains: " + localisedText.Count + " entries");

        }
        catch
        {
            Debug.Log("File not loaded");
        }


    }

    public string GetLocalisedValue(string key)
    {
        string result = missingTextString;
        if (localisedText.ContainsKey(key))
        {
            result = localisedText[key];
        }

        return result;

    }

    [System.Serializable]
    public class Language
    {
        public string current;
        public string lastLang;
    }

    IEnumerator TestURL(string url2)
    {

        Debug.Log(url2);
        WWW www = new WWW(url2);
        yield return url2;

        if (www.error == null)
        {
            Debug.Log("error null");

        }
        else
        {
            Debug.Log("My error :" + www.error);
        }
    }
}
