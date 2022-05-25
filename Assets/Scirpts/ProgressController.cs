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

    public void UpdateProgressInJson(string symbolName)
    {
        if (progress.scannedSymbols.Contains(symbolName))
        {

        }
        else
        {
            progress.scannedSymbols.Add(symbolName);
            dataController.SaveIntoJson(progress, progressFileName);
        }
    }
}

public class ProgressObject
{
    public List<string> scannedSymbols = new List<string>();
}