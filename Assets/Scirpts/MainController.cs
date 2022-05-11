using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using static TempleData;
using System.Linq;
using System;

public class MainController : MonoBehaviour
{
    public static MainController Instance;


    private string detectedSymbolName;

    private SystemLanguage systemLanguage = SystemLanguage.hu;

    private TempleData[] allTempleData;

    private TempleData currentTempleData;

    private TempleData currentTempleDataForGroups;

    private EndpointReader endpointReader;

    public DataController dataController;

    public bool isInitializing = true;

    bool hasInternetConnection = false;

    public bool isDownloading=false;
    public int downloadTarget;
    public int downloadCompleted;

    Dictionary<string,Texture2D> symbolTextures;

    public static event Action<bool> OnDownloadStateChanged = delegate { };

    public KeyValuePair<string, SymbolGroup> chosenSymbol;

    private void Awake()
    {

        CreateSingleton();
        CheckConnection();
        endpointReader = gameObject.GetComponent<EndpointReader>();
        dataController = gameObject.GetComponent<DataController>();
        symbolTextures = new Dictionary<string, Texture2D>();
        if (hasInternetConnection)
        {
            StartCoroutine(endpointReader.GetAllTempleData(ProcessAllTempleData));
        }
        //This part will not be needed maybe??
        else
        {
            if (PlayerPrefs.HasKey("initialSetup"))
            {

                isInitializing = false;
            }
        }
        

    }

    private void Update()
    {
        if (downloadCompleted == downloadTarget && isDownloading)
        {
            DownloadEnded();
        }
    }

    void CheckConnection()
    {

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("No internet Connection!");
            if (PlayerPrefs.HasKey("initialSetup"))
            {
                Debug.Log("Would you like to start offline mode?");
            }
        }
        else
        {
            Debug.Log("Internet connection available");
            hasInternetConnection = true;
        }
    }

    public void ProcessAllTempleData (TempleData[] allTempleData)
    {
        dataController.SaveIntoJson(allTempleData, "AllTempledata.json");
        // For first time if there is no LocalTempleData.json and hast ro be created from scratch
        if (!File.Exists(Application.persistentDataPath + "/" + "LocalTempleData.json"))
        {
            LocalTempleData[] allLocalTempleData = new LocalTempleData[allTempleData.Length];
            int counter = 0;
            foreach (TempleData templeData in allTempleData)
            {
                if (templeData!= null)
                {
                    StartCoroutine(endpointReader.GetImage(templeData.cover_image, templeData.name, SaveImageLocally));
                    currentTempleDataForGroups = templeData;
                    SaveSymbolGroups(templeData);
                    LocalTempleData newLocalTempleData = new LocalTempleData(templeData.updated_at, templeData.name, false);
                    allLocalTempleData[counter] = newLocalTempleData;
                    counter++;
                }
                
            }
            dataController.SaveIntoJson(allLocalTempleData, "LocalTempleData.json");
        }
        else
        {
            string jsonString = dataController.LoadJsonFile("LocalTempleData.json");
            LocalTempleData[] allLocalTempleData = JsonConvert.DeserializeObject<LocalTempleData[]>(jsonString);
            foreach (TempleData templeData in allTempleData)
            {
                if (templeData != null)
                {
                    string name = templeData.name;
                    bool exists = false;
                    int index = 999999;
                    for (int i = 0; i < allLocalTempleData.Length; i++)
                    {
                        if (allLocalTempleData[i].name == name)
                        {
                            exists = true;
                            index = i;
                            break;
                        }
                    }
                    if (exists)
                    {
                        string updated_at = templeData.updated_at;
                        if (updated_at != allLocalTempleData[index].updated_at)
                        {
                            StartCoroutine(endpointReader.GetImage(templeData.cover_image, templeData.name, SaveImageLocally));
                            currentTempleDataForGroups = templeData;
                            SaveSymbolGroups(templeData);
                            allLocalTempleData[index].downloaded = false;
                            allLocalTempleData[index].updated_at = templeData.updated_at;
                            foreach (Symbol symbol in templeData.symbol_groups["csillag"].symbols)
                            {
                                PlayerPrefs.DeleteKey(templeData.name + "/" + symbol.symbol_name);
                            }
                        }
                    }
                    else
                    {
                        StartCoroutine(endpointReader.GetImage(templeData.cover_image, templeData.name, SaveImageLocally));
                        currentTempleDataForGroups = templeData;
                        SaveSymbolGroups(templeData);
                        // Todo some specific testing needed here, I am not 100% sure about the indexing, nope it is not gonna work like this

                        allLocalTempleData.Append(new LocalTempleData(templeData.updated_at, templeData.name, false));

                    }
                }
               

            }
            dataController.SaveIntoJson(allLocalTempleData, "LocalTempleData.json");

        }
     
        if (!PlayerPrefs.HasKey("initialSetup"))
        {
            PlayerPrefs.SetString("initialSetup", "yes");
        }

        isInitializing = false;
       
    }

    public void StartDownload()
    {
        OnDownloadStateChanged(true);
        isDownloading = true;
    }

    public void DownloadEnded()
    {
        OnDownloadStateChanged(false);
        downloadCompleted = 0;
        downloadTarget = 0;
        isDownloading = false;
    }
    public void SaveImageLocally(byte[] resultBytes, string templeName)
    {

        dataController.SaveImageLocally(resultBytes, templeName, templeName);
  
    }
    public TempleData[] LoadAllTempleData()
    {
        return JsonConvert.DeserializeObject<TempleData[]>(dataController.LoadJsonFile("AllTempledata.json"));
      
    }

    public Dictionary<string,LocalTempleData> LoadAllLocalTempledata()
    {
        LocalTempleData[] allLocalTempleData = JsonConvert.DeserializeObject<LocalTempleData[]>(dataController.LoadJsonFile("LocalTempleData.json"));
        Dictionary<string, LocalTempleData> result = new Dictionary<string, LocalTempleData>();
        foreach (LocalTempleData ltd in allLocalTempleData)
        {
            if (ltd != null)
            {
                result.Add(ltd.name, ltd);
            }
            
        }
        return result;
    }

    private void CreateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetDetectedSymbolName(string symbolName)
    {
        this.detectedSymbolName = symbolName;
    }

    public string GetDetectedSymbolName()
    {
        return detectedSymbolName;
    }

    public TempleData[] GetAllTempleData()
    {
        return allTempleData;
    }

    public void SetAllTempleData(TempleData[] allTempleData)
    {
        this.allTempleData = allTempleData;
    }

    public TempleData getCurrentTempleData()
    {
        return currentTempleData;
    }

    public void setCurrentTempleData(TempleData currentTempleData)
    {
        this.currentTempleData = currentTempleData;
    }
    
    public int getNumbeOfTemples()
    {
        return allTempleData.Length;
    }

    public void GetImage(string path,string name, System.Action<byte[],string> callback)
    {
        StartCoroutine(endpointReader.GetImage(path,name,callback));
    }
    public void GetImage(string path, string name, string fileName, System.Action<byte[], string, string> callback)
    {
        StartCoroutine(endpointReader.GetImage(path, name, fileName, callback));
    }

    public void GetAudio(string path, string name, System.Action<byte[], string> callback) {

        StartCoroutine(endpointReader.GetAudio(path, name, callback));
    }

    public byte[] GetImageLocaly(string templeName, string fileName, string extension = ".png")
    {
        return dataController.LoadImageLocally(templeName,fileName, extension);
    }

    public enum SystemLanguage
    {
        uk,
        sk,
        ro,
        hu,
        en,
        cz,
        hr,
        at
    }

    public void SetSystemLanguage(SystemLanguage newSystemLanguage)
    {
        systemLanguage = newSystemLanguage;
    }
    public SystemLanguage GetSystemLanguage()
    {
        return systemLanguage;
    }


    public void SaveLocalTempleData(Dictionary<string,LocalTempleData>  allLocalTempleData)
    {
        LocalTempleData[] dataToSave = new LocalTempleData[allLocalTempleData.Count];
        int index = 0;
        foreach(LocalTempleData ldt in allLocalTempleData.Values)
        {
            dataToSave[index] = ldt;
            index++;
        }
        dataController.SaveIntoJson(dataToSave, "LocalTempleData.json");
    }

    public int GetNumberOfSymbolsVisited(TempleData templeData)
    {
        int counter = 0;
        Debug.Log(templeData + " symbol");
        foreach (Symbol symbol in templeData.symbol_groups["csillag"].symbols)
        {
            if (PlayerPrefs.HasKey(templeData.name + "/" + symbol.symbol_name))
            {
                counter++;
            }
        }
        return counter;
    }

    public void AddToSymbolTextures(Texture2D symbolTexture, string name)
    {
        symbolTextures[name] = symbolTexture;
    }

    public Dictionary<string, Texture2D> GetSymbolTextures()
    {
        return symbolTextures;
    }
    public void ResetSymbolTexturesList()
    {
        symbolTextures = new Dictionary<string, Texture2D>();
    }

    private void SaveSymbolGroups(TempleData templeData)
    {
        var symbolGroups = templeData.symbol_groups;
        foreach (var symbolGroup in symbolGroups)
        {
            MainController.Instance.GetImage(symbolGroup.Value.path, templeData.name, symbolGroup.Key, SaveGroupImageLocally);
        }
    }


    /// <summary>
    /// / to continue
    /// </summary>
    /// <param name="resultBytes"></param>
    /// <param name="fileName"></param>
    public void SaveGroupImageLocally(byte[] resultBytes, string name, string fileName)
    {
        dataController.SaveImageLocally(resultBytes, name, fileName);
    }


}
