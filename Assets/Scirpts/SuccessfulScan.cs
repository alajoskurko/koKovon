using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static TempleData;

public class SuccessfulScan : MonoBehaviour
{
    [SerializeField]
    GameObject BGPanel, successfullScanPanel,symbolPrefab;
    public AudioClip audioClip;
    private AudioSource audioSource;
    string scannedSymbolName;
    [SerializeField]
    Image symbolImage;
    [SerializeField]
    Animator successAnimator;
    [SerializeField]
    GameObject border;

    TempleData currentTempleData;
    Dictionary<string, SymbolGroup> symbolsGroups = new Dictionary<string, SymbolGroup>();
    [SerializeField]
    GameObject ScanImageController;
    [SerializeField]
    AudioManager audioManager;
    [SerializeField]
    Animator audioPanelAnim,fadeAudioBG;
    [SerializeField]
    GameObject audioBG;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentTempleData = MainController.Instance.getCurrentTempleData();
        symbolsGroups = currentTempleData.symbol_groups;
    }

    public void SuccessfulScanHappened(string scannedSymbol)
    {
        //successfullScanPanel.gameObject.SetActive(true);
        //BGPanel.gameObject.SetActive(false);
        border.gameObject.SetActive(false);
        scannedSymbolName = scannedSymbol;
        StartCoroutine(PlayAnims());
        CreatSymbol();
        StartCoroutine(audioManager.LoadAudioLocaly(MainController.Instance.getCurrentTempleData().name,scannedSymbol));
        ScanImageController scaniImgC = ScanImageController.GetComponent<ScanImageController>();
        scaniImgC.InstantiateAll();
    }

    public IEnumerator PlayAnims()
    {
        //fadeAudioBG.SetTrigger("FadeAudioBG");
        audioBG.SetActive(true);
        yield return new WaitForSeconds(1);
        audioPanelAnim.SetBool("playZoomIn", true);
        successAnimator.SetBool("success", true);
    }
    public IEnumerator LoadAudioLocaly(string templeName, string fileName)
    {
        Debug.LogWarning(templeName + " " + fileName + " belepik");
        string path = Application.persistentDataPath + "/" + templeName + "/" + MainController.Instance.selectedLanguage + "/" + fileName + ".mp3";
        string url = string.Format("file://{0}", path);
        WWW www = new WWW(url);
        yield return www;
        audioClip = www.GetAudioClip(false, false);
        audioSource.clip = audioClip;
        //audioSource.Play();
    }
    private void CreatSymbol()
    {
        Texture2D imageTexture = new Texture2D(512, 512, TextureFormat.PVRTC_RGBA4, false);
        byte[] resultBytes = MainController.Instance.GetImageLocaly(MainController.Instance.getCurrentTempleData().name, scannedSymbolName, ".jpg");
        imageTexture.LoadImage(resultBytes);
        symbolImage.sprite = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2());
        symbolImage.SetNativeSize();
        
        var screenWidth = Screen.width * 70 / 100;
        if (imageTexture.width > screenWidth)
        {
            symbolImage.transform.localScale = new Vector3(1.75f, 1.75f, .45f);
        }
        else
        {
            symbolImage.transform.localScale = new Vector3(1.95f, 1.95f, .65f);
        }
    }

    public void LoadSpecificTempleScene()
    {
        SceneManager.LoadScene("SpecificTempleScene");
    }
}
