using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SwipeDetector;
using static TempleData;

public class TempleSceneController : MonoBehaviour
{
    [SerializeField]
    Text templeNameText;
    [SerializeField]
    TMPro.TMP_Text symbolsDiscoveredText;
    [SerializeField]
    RawImage templeImage;
    [SerializeField]
    Slider slider;
    [SerializeField]
    Button downloadButton;
    TempleData currentTemple;
    string templeName;
    Symbol[] symbols;
    SymbolGroup[] symbolGroups;
    Dictionary<string, SymbolGroup> symbolsGroups = new Dictionary<string, SymbolGroup>();
    [SerializeField]
    GameObject symbolGroupPrefab,groupChoosePanel,groupContainer,mainUIPanel;

    public static TempleSceneController Instance;
    //SymbolGroups symbolGroupss;

    private void Awake()
    {
        Instance = this;
        MainController.OnDownloadStateChanged += SetDownloadButtonState;
        SwipeDetector.OnSwipe += SwipeDetector_OnSwipe;
    }

    void Start()
    {
        currentTemple = MainController.Instance.getCurrentTempleData();
        templeName= currentTemple.name;
        templeNameText.text = templeName;
        symbolsDiscoveredText.text =MainController.Instance.GetNumberOfSymbolsVisited(currentTemple).ToString()+"/"+ GetSymbolsLength().ToString();
        slider.maxValue = GetSymbolsLength();
        slider.value = MainController.Instance.GetNumberOfSymbolsVisited(currentTemple);
        SetDownloadButtonState(MainController.Instance.isDownloading);

        Dictionary<string,LocalTempleData> allLocalTempleData = MainController.Instance.LoadAllLocalTempledata();

        // Todo make this inot a function
        if (allLocalTempleData[templeName].downloaded)
        {
            Texture2D imageTexture = new Texture2D(150, 150, TextureFormat.PVRTC_RGBA4, false);
            byte[] resultBytes = MainController.Instance.GetImageLocaly(templeName, templeName);
            imageTexture.LoadImage(resultBytes);
            templeImage.texture = imageTexture;
            LoadSymbolsForDiscoverScene();
        }
        else
        {
            Texture2D imageTexture = new Texture2D(150, 150, TextureFormat.PVRTC_RGBA4, false);
            byte[] resultBytes = MainController.Instance.GetImageLocaly(templeName, templeName);
            imageTexture.LoadImage(resultBytes);
            templeImage.texture = imageTexture;
            templeImage.color = new Color32(111, 189, 195, 255);
        }
        InstantiateSymbolGroups(groupContainer);
    }
    int GetSymbolsLength()
    {
        int symbolsLength = 0;
        foreach (KeyValuePair<string, SymbolGroup> symbolGroup in currentTemple.symbol_groups)
        {
            symbolsLength += symbolGroup.Value.symbols.Length;
        }
        return symbolsLength;
    }
        //}
        //void int GetSymbolsLength()
        //{
        //    int symbolsLength = 0;
        //    foreach (KeyValuePair<string,SymbolGroup> symbolGroup in currentTemple.symbol_groups)
        //    {
        //        symbolsLength += symbolGroup.Value.symbols.Length;
        //    }
        //    return symbolsLength;
        //}

        public void LoadSymbolsForDiscoverScene()
    {
        foreach (KeyValuePair<string, SymbolGroup> symbolGroup in symbolsGroups)
        {
            foreach (Symbol symbol in symbolGroup.Value.symbols)
            {
                Texture2D imageTexture = new Texture2D(512, 512, TextureFormat.PVRTC_RGBA4, false);
                byte[] resultBytes = MainController.Instance.GetImageLocaly(MainController.Instance.getCurrentTempleData().name, symbol.symbol_name);
                imageTexture.LoadImage(resultBytes);
                MainController.Instance.AddToSymbolTextures(imageTexture, symbol.symbol_name);
            }
        }

    }
    public void LoadSymbolGropus(GameObject parent){
        foreach (KeyValuePair<string, SymbolGroup> symbol in symbolsGroups)
        {
            var newSymbolItem = Instantiate(symbolGroupPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            if (parent != null)
            {
                newSymbolItem.transform.parent = parent.transform;
                newSymbolItem.transform.localScale = new Vector3(1, 1, 1);

            }
            newSymbolItem.GetComponent<SymbolGroupPrefabController>().SetSymbolData(symbol);
        }
    }
    public void ProcessSymbolImage(byte[] resultBytes, string name)
    { 
    }
        public void SetDownloadButtonState(bool state)
    {
        if (state)
        {
            downloadButton.gameObject.SetActive(!state);
        }
        else
        {
            print("HEYHO");
            downloadButton.gameObject.SetActive(true);

        }

    }

    public void ChooseSymbolGroupForScan(KeyValuePair<string,SymbolGroup> symbolGroup)
    {
        MainController.Instance.chosenSymbol = symbolGroup;
        LoadImageDetectionScene();
    }

    public void OnDiscoverButtonHit()
    {
        // should chekc if temple data is downloaded or not
        Dictionary<string, LocalTempleData> allLocalTempleData =  MainController.Instance.LoadAllLocalTempledata();
        if (allLocalTempleData[templeName].downloaded)
        {
            LoadDiscoverScene();
        }
        else
        {
            //Todo probably popup needed or something
            print("Donwload the files first please");
        }
      
    }

    public void OnDownloadButtonHit()
    {
        MainController.Instance.StartDownload();
        MainController.Instance.downloadCompleted = 0;
        MainController.Instance.downloadTarget = GetSymbolsLength();

        //Should download and save the data also it should store that the temple data was downloaded
        foreach (KeyValuePair<string, SymbolGroup> symbolGroup in currentTemple.symbol_groups)
        {
            foreach (Symbol symbol in symbolGroup.Value.symbols)
            {
                MainController.Instance.GetImage(symbol.symbol_path, symbol.symbol_name, SaveSymbolImage);
                foreach (AudioData audioData in symbol.audios)
                {
                    MainController.Instance.GetAudio(audioData.path, audioData.name, SaveSymbolAudio);
                }
            }
        }
        
        // TODO edit this part
        Dictionary<string,LocalTempleData> allLocalTempledata = MainController.Instance.LoadAllLocalTempledata();
        allLocalTempledata[templeName].downloaded = true;
        MainController.Instance.SaveLocalTempleData(allLocalTempledata);     
    }
    public void InstantiateSymbolGroups(GameObject parent)
    {
        foreach (KeyValuePair<string, SymbolGroup> symbol in currentTemple.symbol_groups)
        {
            var newSymbolItem = Instantiate(symbolGroupPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            if (parent != null)
            {
                newSymbolItem.transform.parent = parent.transform;
                newSymbolItem.transform.localScale = new Vector3(1, 1, 1);

            }
            newSymbolItem.GetComponent<SymbolGroupPrefabController>().SetSymbolData(symbol);
        }
    }
    public void OnGoogleMapsButtonHit()
    {
        Application.OpenURL(currentTemple.google_map);
    }

    private void SaveSymbolImage(byte[] resultBytes, string fileName) {

        MainController.Instance.dataController.SaveImageLocally(resultBytes, templeName, fileName);
    }



    private void SaveSymbolAudio(byte[] resultBytes, string fileName)
    {
        MainController.Instance.dataController.SaveAudioLocally(resultBytes, templeName, fileName);
        MainController.Instance.downloadCompleted++;
    }


    public void LoadHomeScreen()
    {
        SwipeDetector.OnSwipe -= SwipeDetector_OnSwipe;
        MainController.OnDownloadStateChanged -= SetDownloadButtonState;
        SceneManager.LoadScene("TempleSelectionScene");
        // NOTE, what happens here is when we leave the temple specific scene, if we dont unsubscribe
        // than when we start a new download from a different page, it will trigger event on this listener as well
        // but this object will be destroyed by then therefore give error and freeze the application
       
    }

    public void LoadDiscoverScene()
    {
        SwipeDetector.OnSwipe -= SwipeDetector_OnSwipe;
        MainController.OnDownloadStateChanged -= SetDownloadButtonState;
        SceneManager.LoadScene("DiscoverScene");
    }

    private void SwipeDetector_OnSwipe(SwipeData data)
    {
        if (data.Direction == SwipeDirection.Right)
        {
            LoadHomeScreen();
        }
    }

    public void ShowSymbolGroupsForScan()
    {
        groupChoosePanel.SetActive(true);
        mainUIPanel.SetActive(false);
    }

    public void LoadImageDetectionScene()
    {
        SwipeDetector.OnSwipe -= SwipeDetector_OnSwipe;
        MainController.OnDownloadStateChanged -= SetDownloadButtonState;
        SceneManager.LoadScene("ImageDetectionScene");
    }

}
