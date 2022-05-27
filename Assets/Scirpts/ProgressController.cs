using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class ProgressController : MonoBehaviour
{
    const string progressFileName = "Progress.json";
    DataController dataController;
    public ProgressObject progress;

    
    // Start is called before the first frame update
    void Start()
    {
        dataController = gameObject.GetComponent<DataController>();
        if (System.IO.File.Exists(Application.persistentDataPath + "/" + progressFileName))
        {
            string jsonString = dataController.LoadJsonFile(progressFileName);
            progress = JsonConvert.DeserializeObject<ProgressObject>(jsonString);
        }
        else
        {
            dataController.SaveIntoJson(new ProgressObject(), progressFileName);
            string jsonString = dataController.LoadJsonFile(progressFileName);
            progress = JsonConvert.DeserializeObject<ProgressObject>(jsonString);
        }
        
    }

    public ProgressObject LoadProgressObject()
    {

        return progress;
    }

    public void UpdateProgressInJson(string symbolName, string currentTemple)
    {
        if (progress.scannedSymbols.ContainsKey(currentTemple)) {
            if (progress.scannedSymbols[currentTemple].Contains(symbolName))
            {

            }
            else {
                progress.scannedSymbols[currentTemple].Add(symbolName);
                dataController.SaveIntoJson(progress, progressFileName);
            }
            
        }
        else
        {
            progress.scannedSymbols.Add(currentTemple, new List<string>());
            progress.scannedSymbols[currentTemple].Add(symbolName);
            dataController.SaveIntoJson(progress, progressFileName);
        }
    }
}

public class ProgressObject
{
   public Dictionary<string, List<string>> scannedSymbols = new Dictionary<string, List<string>>();
}