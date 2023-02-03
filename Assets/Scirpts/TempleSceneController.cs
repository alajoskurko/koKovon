﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
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
    Text warningText;
    [SerializeField]
    Text netErrorText;
    [SerializeField]
    Text playWarningText;
    [SerializeField]
    GameObject downloadWarningPanel;
    [SerializeField]
    GameObject donwloadInProgressPanel;
    [SerializeField]
    Text donwloadInProgressText;
    [SerializeField]
    GameObject netErrorPanel;
    [SerializeField]
    GameObject playWarningPanel;
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
    GameObject symbolGroupPrefab, groupChoosePanel, groupContainer, mainUIPanel;
    [SerializeField]
    Slider scanSliderProgress;
    [SerializeField]
    Animator downloadButtonAnimator;
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    GameObject audioPlayButton;
    [SerializeField]
    GameObject LoadingImage;
    [SerializeField]
    TMPro.TMP_Text chooseShapeText;
    [SerializeField]
    public GameObject progressObj;
    [SerializeField]
    Canvas mainCanvas;
    AudioClip myClip;

    Dictionary<string, string> downloadWarning = new Dictionary<string, string>() { { "hu", "Töltsd le a fájlokat!" }, { "ro", "Descărcare fișiere" }, { "en", "Download the files" } };
    Dictionary<string, string> downloadInProgressWarning = new Dictionary<string, string>() { { "hu", "Letöltés folyamatban..." }, { "ro", "Descărcare..." }, { "en", "Download in progress.." } };
    Dictionary<string, string> downloadError = new Dictionary<string, string>() { { "hu", "Letöltési hiba!" }, { "ro", "Eroare de descărcare!" }, { "en", "Download error!" } };
    Dictionary<string, string> playWarning = new Dictionary<string, string>() { { "hu", "Olvasd be az illusztrációkat!" }, { "ro", "Scanare" }, { "en", "Scanning" } };
    Dictionary<string, string> shapeTexts = new Dictionary<string, string>() { { "hu", "Válassz egy formát!" }, { "ro", "Selectarea formei!" }, { "en", "Choose the form!" } };

    public static TempleSceneController Instance;
    //SymbolGroups symbolGroupss;

    [SerializeField]
    Animator animator;

    private void Awake()
    {
        Instance = this;
        MainController.OnDownloadStateChanged += SetDownloadButtonState;
        SwipeDetector.OnSwipe += SwipeDetector_OnSwipe;
        if((float) Screen.height / (float) Screen.width < 1.55)
        {
            mainCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 800);
            Debug.Log((float) Screen.height / (float) Screen.width + "tablet szeles");
        }
        
    }

    void Start()
    {
        chooseShapeText.text = shapeTexts[MainController.Instance.selectedLanguage];
        currentTemple = MainController.Instance.getCurrentTempleData();
        templeName= currentTemple.name;
        warningText.text = downloadWarning[MainController.Instance.selectedLanguage];
        netErrorText.text = downloadError[MainController.Instance.selectedLanguage];
        playWarningText.text = playWarning[MainController.Instance.selectedLanguage];
        donwloadInProgressText.text = downloadInProgressWarning[MainController.Instance.selectedLanguage];
        //GetComponent<Animation>().Play("scanButtonFlashing");
        if (MainController.Instance.selectedLanguage == "hu")
        {
            templeNameText.text = templeName;
        }
        else
        {
            foreach (var templeDetail in currentTemple.temple_details)
            {
                if (templeDetail.lang == MainController.Instance.selectedLanguage)
                {
                    templeNameText.text = templeDetail.name;
                }
            }
        }

        slider.maxValue = GetSymbolsLength();
        slider.value = MainController.Instance.GetNumberOfSymbolsVisited(currentTemple);
        SetDownloadButtonState(MainController.Instance.isDownloading);
        if (MainController.Instance.progressController.progress.scannedSymbols.ContainsKey(currentTemple.name))
        {
            scanSliderProgress.value = MainController.Instance.progressController.progress.scannedSymbols[currentTemple.name].Count;
            symbolsDiscoveredText.text = MainController.Instance.progressController.progress.scannedSymbols[currentTemple.name].Count.ToString()+"/"+ GetSymbolsLength().ToString();
        }
        else
        {
            symbolsDiscoveredText.text = "0/" + GetSymbolsLength().ToString();
        }
        Dictionary<string,LocalTempleData> allLocalTempleData = MainController.Instance.LoadAllLocalTempledata();

        // Todo make this inot a function
        if (allLocalTempleData[templeName].downloaded[MainController.Instance.selectedLanguage])
        {
            Texture2D imageTexture = new Texture2D(150, 150, TextureFormat.RGB565, false);
            byte[] resultBytes = MainController.Instance.GetImageLocaly(templeName, templeName);
            imageTexture.LoadImage(resultBytes);
            templeImage.texture = imageTexture;
            templeImage.SetNativeSize();
            LoadSymbolsForDiscoverScene();
            StartCoroutine(LoadAudioLocaly());
        }
        else
        {
            Texture2D imageTexture = new Texture2D(150, 150, TextureFormat.RGB565, false);
            byte[] resultBytes = MainController.Instance.GetImageLocaly(templeName, templeName);
            imageTexture.LoadImage(resultBytes);
            templeImage.texture = imageTexture;
            templeImage.color = new Color32(255, 255, 255, 255);
            templeImage.SetNativeSize();
        }
        InstantiateSymbolGroups(groupContainer);
        
    }
    private void Update()
    {
        if (audioSource.isPlaying)
        {
            audioPlayButton.GetComponent<Image>().enabled = false;
            audioPlayButton.transform.GetChild(0).gameObject.SetActive(true);
            //Debug.Log(audioSource.clip.length);
        }
        else
        {
            audioPlayButton.GetComponent<Image>().enabled = true;
            audioPlayButton.transform.GetChild(0).gameObject.SetActive(false);
            
        }
    }
    int GetSymbolsLength()
    {
        var akarmi = currentTemple.symbol_groups;
        int symbolsLength = 0;
        foreach (KeyValuePair<string, SymbolGroup> symbolGroup in currentTemple.symbol_groups)
        {
            symbolsLength += symbolGroup.Value.symbols.Length;
        }
        return symbolsLength;
    }

    int GetSymbolAudiosLength()
    {
        int audioLength = 0;
        foreach (KeyValuePair<string, SymbolGroup> symbolGroup in currentTemple.symbol_groups)
        {
            foreach (var symbol in symbolGroup.Value.symbols)
            {
                foreach (var audio in symbol.audios)
                {
                    if(audio.lang == MainController.Instance.selectedLanguage)
                    {
                        audioLength++;
                    }
                } 
            } 
        }
        return audioLength;
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
                Texture2D imageTexture = new Texture2D(512, 512, TextureFormat.RGB565, false);
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

        public void SetDownloadButtonState(bool state)
    {
        if (state && downloadButtonAnimator)
        {
            downloadButtonAnimator.SetBool("playWarning", false);
            downloadButtonAnimator.SetBool("isDownloading", true);
            Debug.LogWarning("toltodik");
            //downloadButton.gameObject.SetActive(!state);
        }
        else
        {
            if (downloadButtonAnimator)
            {
                downloadButtonAnimator.SetBool("isDownloading", false);
            }
            if (downloadWarningPanel)
            {
                downloadWarningPanel.gameObject.SetActive(false);
            }
            
            //downloadButton.gameObject.SetActive(true);
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
        if (allLocalTempleData[templeName].downloaded[MainController.Instance.selectedLanguage])
        {
            LoadDiscoverScene();
        }
        else
        {
            //Todo probably popup needed or something
            print("Donwload the files first please");
            downloadWarningPanel.gameObject.SetActive(true);
            downloadButtonAnimator.SetBool("playWarning", true);
        }
      
    }

    public void OnDownloadButtonHit()
    {
        downloadWarningPanel.gameObject.SetActive(false);
        if (MainController.Instance.isDownloading)
        {
            return;
        }
        MainController.Instance.StartDownload();
        MainController.Instance.downloadCompleted = 0;
        donwloadInProgressPanel.SetActive(true);
        /// download target must be the length of the symbols array + the lang of the audios by the selected language + 1(temple intro audio)
        MainController.Instance.downloadTarget = GetSymbolsLength() + GetSymbolAudiosLength() + 1;
        Debug.Log("download target :   " + MainController.Instance.downloadTarget);

        //Should download and save the data also it should store that the temple data was downloaded
        foreach (KeyValuePair<string, SymbolGroup> symbolGroup in currentTemple.symbol_groups)
        {
            foreach (Symbol symbol in symbolGroup.Value.symbols)
            {
                MainController.Instance.GetImage(symbol.symbol_path, symbol.symbol_name, SaveSymbolImage);
                foreach (AudioData audioData in symbol.audios)
                {
                    if(audioData.lang == MainController.Instance.selectedLanguage)
                    {
                        MainController.Instance.GetAudio(audioData, symbol.symbol_name, SaveSymbolAudio);
                    }
                    
                }
            }
        }
        string hungarianName = "";
        AudioData audioData2 = new AudioData();
        audioData2.path = currentTemple.audio_intro;
        audioData2.name = currentTemple.name;
        audioData2.lang = "hu";
        hungarianName = currentTemple.name;

        List<AudioData> audioDatas = new List<AudioData>();
        audioDatas.Add(audioData2);
        foreach (var detail in currentTemple.temple_details)
        {
            AudioData audioData = new AudioData();
            audioData.path = detail.audio_intro;
            audioData.name = detail.name;
            audioData.lang = detail.lang;
            audioDatas.Add(audioData);
        }

        foreach (var audiodata in audioDatas)
        {
            MainController.Instance.GetAudio(audiodata, hungarianName, SaveSymbolAudio);
        }
        

        // TODO edit this part
        //Dictionary<string,LocalTempleData> allLocalTempledata = MainController.Instance.LoadAllLocalTempledata();
        //allLocalTempledata[templeName].downloaded = true;
        //MainController.Instance.SaveLocalTempleData(allLocalTempledata);     
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
        if (resultBytes == null)
        {
            downloadButtonAnimator.SetBool("isDownloading", false);
            MainController.Instance.isDownloading = false;
            downloadWarningPanel.gameObject.SetActive(false);
            netErrorPanel.SetActive(true);
            return;
        }
        MainController.Instance.dataController.SaveImageLocally(resultBytes, templeName, fileName);
        MainController.Instance.downloadCompleted++;
        Debug.Log(MainController.Instance.downloadCompleted + " downloaded files numbe"); 
    }

    private void SaveSymbolAudio(TempleData.AudioData audiodata, byte[] resultBytes, string fileName)
    {
        if(audiodata == null)
        {
            downloadButtonAnimator.SetBool("isDownloading", false);
            downloadWarningPanel.gameObject.SetActive(false);
            netErrorPanel.SetActive(true);
            MainController.Instance.isDownloading = false;
            return;
        }
        MainController.Instance.downloadCompleted++;
        MainController.Instance.dataController.SaveAudioLocally(audiodata.lang,resultBytes, templeName, fileName);
        
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

    public void showPlayAnim()
    {
        donwloadInProgressPanel.SetActive(false);
        StartCoroutine(ScanWarning());
        animator.SetTrigger("scanFlash");
    }

    IEnumerator ScanWarning()
    {
        playWarningPanel.SetActive(true);
        netErrorPanel.SetActive(false);
        yield return new WaitForSeconds(2.7f);
        playWarningPanel.SetActive(false);
    }

    public void ShowSymbolGroupsForScan()
    {
        Dictionary<string, LocalTempleData> allLocalTempleData = MainController.Instance.LoadAllLocalTempledata();
        if (!allLocalTempleData[templeName].downloaded[MainController.Instance.selectedLanguage]){
            OnDownloadButtonHit();
            return;
        }
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
            
        if (allLocalTempleData[templeName].downloaded[MainController.Instance.selectedLanguage])
        {
            groupChoosePanel.SetActive(true);
            mainUIPanel.SetActive(false);
        }
        else
        {
            //Todo probably popup needed or something
            print("Donwload the files first please");
            downloadWarningPanel.gameObject.SetActive(true);
            netErrorPanel.gameObject.SetActive(false);
            warningText.gameObject.SetActive(true);
            downloadButtonAnimator.SetBool("playWarning", true);
        }

        
    }

    public void PlayIntroMusic()
    {
        // should chekc if temple data is downloaded or not
        Dictionary<string, LocalTempleData> allLocalTempleData = MainController.Instance.LoadAllLocalTempledata();
        if (allLocalTempleData[templeName].downloaded[MainController.Instance.selectedLanguage])
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
                audioPlayButton.GetComponent<Image>().enabled = true;
                audioPlayButton.transform.GetChild(0).gameObject.SetActive(false);
            }
            else
            {
                audioPlayButton.GetComponent<Image>().enabled = false;
                audioPlayButton.transform.GetChild(0).gameObject.SetActive(true);
                audioSource.Play();
                //StartCoroutine(LoadAudioLocaly());
            }
            
        }
        else
        {
            downloadWarningPanel.gameObject.SetActive(true);
            netErrorPanel.gameObject.SetActive(false);
            warningText.gameObject.SetActive(true);
            downloadButtonAnimator.SetBool("playWarning", true);
        }
       
    }
    public void HideWarningPanel ()
    {
        downloadWarningPanel.gameObject.SetActive(false);
    }
    public IEnumerator LoadAudioLocaly()
    {
        string path = Application.persistentDataPath + "/" + currentTemple.name + "/" + MainController.Instance.selectedLanguage + "/" + currentTemple.name + ".mp3";
        string url = string.Format("file://{0}", path);
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                myClip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = myClip;
            }
        }
        //WWW www = new WWW(url);

        //    yield return www;

        //AudioClip audioClip = www.GetAudioClip(false, false, AudioType.mp);
       
        //Debug.Log(audioClip.loadState + " audioClip.isReadyToPlay");
        //Debug.Log(audioClip.length + " audio length");
        //Debug.Log(audioSource.clip.length + " audio length");
        //audioSource.Play();
    }

    public void LoadImageDetectionScene()
    {
        SwipeDetector.OnSwipe -= SwipeDetector_OnSwipe;
        MainController.OnDownloadStateChanged -= SetDownloadButtonState;
        LoadingImage.gameObject.SetActive(true);
        SceneManager.LoadSceneAsync("ImageDetectionScene");  
    }

}
