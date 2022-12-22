using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using static TempleData;
using System.Linq;
using System;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    public static MainController Instance;
    private string _selectedLanguage = "hu";

    public string selectedLanguage
    {
        get
        {
            return _selectedLanguage;
        }

        set
        {
            _selectedLanguage = value;
        }
    }
    public const string hunLanguage = "hu", roLanguage = "ro", enLanguage = "en";
    private string detectedSymbolName;

    private SystemLanguage systemLanguage = SystemLanguage.hu;

    private TempleData[] allTempleData;

    private TempleData currentTempleData;

    private TempleData currentTempleDataForGroups;

    private EndpointReader endpointReader;

    public DataController dataController;

    public bool isInitializing = true;

    public bool hasInternetConnection = false;

    public bool isDownloading=false;
    public int downloadTarget;
    public int downloadCompleted;

    Dictionary<string,Texture2D> symbolTextures;

    public static event Action<bool> OnDownloadStateChanged = delegate { };

    public KeyValuePair<string, SymbolGroup> chosenSymbol;

    public ProgressController progressController;

    public float templeSelectorScrollrectPositionY = 1;

    [SerializeField]
    private GameObject netErrorText;

    private void Awake()
    {
        
        CreateSingleton();
        CheckConnection();
        endpointReader = gameObject.GetComponent<EndpointReader>();
        dataController = gameObject.GetComponent<DataController>();
        progressController = gameObject.GetComponent<ProgressController>();
        symbolTextures = new Dictionary<string, Texture2D>();
        Debug.LogWarning(hasInternetConnection + " hasInternetConnection ");
        if (hasInternetConnection)
        {
            StartCoroutine(endpointReader.GetAllTempleData(ProcessAllTempleData));
        }
                //This part will not be needed maybe??
        else
        {
            netErrorText.SetActive(true);
            StartCoroutine(CheckIfUserTurnedOnTheNet());
            if (PlayerPrefs.HasKey("initialSetup"))
            {

                //isInitializing = false;
            }
        }
        

    }

    private IEnumerator CheckIfUserTurnedOnTheNet ()
    {
        Debug.Log("CheckIfUserTurnedOnTheNet");
        yield return new WaitForSecondsRealtime(2);
        CheckConnection();
        if (hasInternetConnection)
        {
            netErrorText.SetActive(false);
            StopCoroutine(CheckIfUserTurnedOnTheNet());
            StartCoroutine(endpointReader.GetAllTempleData(ProcessAllTempleData));
        }
        else
        {
            StartCoroutine(CheckIfUserTurnedOnTheNet());
        }
        
    }

    private void Update()
    {
        if (downloadCompleted > downloadTarget-1 && isDownloading)
        {
            DownloadEnded();
            TempleSceneController.Instance.progressObj.SetActive(false);
        }
        else if (isDownloading)
        {
            float num = (downloadCompleted * 100) / (downloadTarget - 1);
            TempleSceneController.Instance.progressObj.SetActive(true);
            TempleSceneController.Instance.progressObj.gameObject.transform.GetChild(0).GetComponent<Slider>().value= num / 100;
        }
    }

    public void ChangeLanguage(string languageParam)
    {
        selectedLanguage = languageParam;
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
            Debug.LogWarning(" toltesi hiba file doesnt exist");
            LocalTempleData[] allLocalTempleData = new LocalTempleData[allTempleData.Length];
            int counter = 0;
            foreach (TempleData templeData in allTempleData)
            {
                if (templeData!= null)
                {
                    Debug.LogWarning(" toltesi hiba file doesnt exist templadata nem null");

                    StartCoroutine(endpointReader.GetImage(templeData.cover_image, templeData.name, SaveImageLocally));
                    currentTempleDataForGroups = templeData;
                    SaveSymbolGroups(templeData);
                    Dictionary<string, bool> downloadedLangs = new Dictionary<string, bool>()
                    {
                        { "hu",false },
                        { "ro",false },
                        { "en",false }
                    };
                    LocalTempleData newLocalTempleData = new LocalTempleData(templeData.updated_at, templeData.name, downloadedLangs);
                    allLocalTempleData[counter] = newLocalTempleData;
                    counter++;
                }
                
            }
            dataController.SaveIntoJson(allLocalTempleData, "LocalTempleData.json");
        }
        else
        {
            Debug.LogWarning(" toltesi hiba file exist");

            string jsonString = dataController.LoadJsonFile("LocalTempleData.json");
            LocalTempleData[] allLocalTempleData = JsonConvert.DeserializeObject<LocalTempleData[]>(jsonString);
            foreach (TempleData templeData in allTempleData)
            {
                if (templeData != null)
                {
                    Debug.LogWarning(" toltesi hiba file exist templedata nem null");
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
                        Debug.LogWarning(" toltesi hiba file exist templedata nem null exists");
                        string updated_at = templeData.updated_at;
                        if (updated_at != allLocalTempleData[index].updated_at)
                        {
                            StartCoroutine(endpointReader.GetImage(templeData.cover_image, templeData.name, SaveImageLocally));
                            currentTempleDataForGroups = templeData;
                            SaveSymbolGroups(templeData);
                          
                            allLocalTempleData[index].downloaded = new Dictionary<string, bool>(){
                                {"hu",false },
                                {"ro",false },
                                {"en",false }
                            };
                            allLocalTempleData[index].updated_at = templeData.updated_at;
                            foreach (KeyValuePair<string, SymbolGroup> symbolGroup in templeData.symbol_groups)
                            {
                                foreach (Symbol symbol in symbolGroup.Value.symbols)
                                {
                                    PlayerPrefs.DeleteKey(templeData.name + "/" + symbol.symbol_name);
                                }
                            }
                              
                        }
                    }
                    else
                    {
                        Debug.LogWarning(" toltesi hiba file exist templedata nem nem exists");
                        StartCoroutine(endpointReader.GetImage(templeData.cover_image, templeData.name, SaveImageLocally));
                        currentTempleDataForGroups = templeData;
                        SaveSymbolGroups(templeData);
                        // Todo some specific testing needed here, I am not 100% sure about the indexing, nope it is not gonna work like this
                        Dictionary<string, bool> newDict = new Dictionary<string, bool>() {
                            {"hu",false },
                            {"ro",false },
                            {"en",false }
                        };
                        allLocalTempleData.Append(new LocalTempleData(templeData.updated_at, templeData.name, newDict));

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
        TempleSceneController templeSceneController = GameObject.Find("TempleSceneController").GetComponent<TempleSceneController>();
        templeSceneController.showPlayAnim();
        OnDownloadStateChanged(false);
        downloadCompleted = 0;
        downloadTarget = 0;
        isDownloading = false;
        Dictionary<string, LocalTempleData> allLocalTempledata = MainController.Instance.LoadAllLocalTempledata();
        allLocalTempledata[currentTempleData.name].downloaded[MainController.Instance.selectedLanguage] = true;
        MainController.Instance.SaveLocalTempleData(allLocalTempledata);
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

    public void GetAudio(TempleData.AudioData audioData, string name, System.Action<TempleData.AudioData, byte[], string> callback) {

        StartCoroutine(endpointReader.GetAudio(audioData,audioData.path, name, callback));
    }
    //public void GetAudio(string path, string name, System.Action<string, string> callback)
    //{

    //    StartCoroutine(endpointReader.GetAudio(audioData, audioData.path, name, callback));
    //}

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
        //Debug.Log(templeData + " symbol");
        foreach (KeyValuePair<string, SymbolGroup> symbolGroup in templeData.symbol_groups)
        {
            foreach (Symbol symbol in symbolGroup.Value.symbols)
            {
                if (PlayerPrefs.HasKey(templeData.name + "/" + symbol.symbol_name))
                {
                    counter++;
                }
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
