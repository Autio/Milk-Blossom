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
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        if(File.Exists(filePath))
        {
            string dataAsJSON = File.ReadAllText(filePath);
            LocalisationData loadedData = JsonUtility.FromJson<LocalisationData>(dataAsJSON);

            for (int i = 0; i < loadedData.items.Length; i++)
            {
                localisedText.Add(loadedData.items[i].key, loadedData.items[i].value);
            }

        } else
        {
            Debug.LogError("Cannot find file!");
        }
        Debug.Log("Data loaded, dictionary contains: " + localisedText.Count + " entries");
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
}
