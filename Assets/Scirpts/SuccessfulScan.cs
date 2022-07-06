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

    TempleData currentTempleData;
    Dictionary<string, SymbolGroup> symbolsGroups = new Dictionary<string, SymbolGroup>();
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentTempleData = MainController.Instance.getCurrentTempleData();
        symbolsGroups = currentTempleData.symbol_groups;
    }

    public void SuccessfulScanHappened(string scannedSymbol)
    {
        successfullScanPanel.gameObject.SetActive(true);
        BGPanel.gameObject.SetActive(false);
        scannedSymbolName = scannedSymbol;
        CreatSymbol();
        StartCoroutine(LoadAudioLocaly(MainController.Instance.getCurrentTempleData().name,scannedSymbol));
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
        audioSource.Play();
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
            symbolImage.transform.localScale = new Vector3(.45f, .45f, .45f);
        }
        else
        {
            symbolImage.transform.localScale = new Vector3(.65f, .65f, .65f);
        }
    }

    public void LoadSpecificTempleScene()
    {
        SceneManager.LoadScene("SpecificTempleScene");
    }
}
