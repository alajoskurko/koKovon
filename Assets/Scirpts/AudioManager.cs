using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{

                
    public AudioClip audioClip;
    [SerializeField]
    private Text clipTimeText;
    [SerializeField]
    private TMPro.TMP_Text clipNameText;
    [SerializeField]
    private Slider slider;
    [SerializeField]
    private RawImage audioImage;
    [SerializeField]
    private GameObject playButton;
    [SerializeField]
    private GameObject stopButton;
    


    private AudioSource audioSouce;
    private int fullLength;
    private int playTime;
    private int seconds;
    private int minutes;
    private string symbolName;


    
    void Start()
    {
        audioSouce = GetComponent<AudioSource>();
        ProcessAudiFile();
     

    }

    private void Update()
    {
        slider.value = playTime;
    }

    public void PlayMusic()
    {
        if (audioSouce.isPlaying)
        {
            audioSouce.Stop();
            stopButton.SetActive(false);
            playButton.SetActive(true);
          
            StopCoroutine(WaitForMusicToEnd());
        }
        else
        {
            audioSouce.time = playTime;
            audioSouce.Play();
            stopButton.SetActive(true);
            playButton.SetActive(false);
            StartCoroutine(WaitForMusicToEnd());
        }

       
    }

    private IEnumerator WaitForMusicToEnd()
    {
        while (audioSouce.isPlaying)
        {
            playTime = (int)audioSouce.time;
            ShowPlaytime();
            yield return null;
        }
        if (playTime == fullLength)
        {
            AudioClipOver();
        }
       
    }

    public void ProcessAudiFile()
    {

        //symbolName = MainController.Instance.getImgeName();
        //string templeName = MainController.Instance.getCurrentTempleData().name;
        string templeName = "Test Temple";
      //  symbolName = MainController.Instance.GetDetectedSymbolName();
      //  Texture2D imageTexture = new Texture2D(512, 512, TextureFormat.PVRTC_RGBA4, false);
      //  byte[] resultBytes = MainController.Instance.dataController.LoadImageLocally(templeName, symbolName);
      //  imageTexture.LoadImage(resultBytes);
     //   audioImage.texture = imageTexture;
        StartCoroutine(LoadAudioLocaly(templeName, "1_Kapolna_HU"));
     //   clipNameText.text = symbolName;
  
    }
    //Todod delete not working currently
    public IEnumerator LoadAudioLocaly(string templeName, string fileName)
    {
        string path = Application.persistentDataPath + "/" + templeName + "/" + MainController.Instance.selectedLanguage + "/" + fileName + ".mp3";
        string url = string.Format("file://{0}", path);
        WWW www = new WWW(url);
        yield return www;
        audioClip = www.GetAudioClip(false, false);
        audioSouce.clip = audioClip;
        audioSouce.time = playTime;
        fullLength = (int)audioClip.length;
        slider.maxValue = fullLength;
        ShowPlaytime();

    }


    void ShowPlaytime()
    {

        seconds = playTime % 60;
        minutes = (playTime / 60) % 60;
        clipTimeText.text = minutes + ":" + seconds.ToString("D2") + "/" + ((fullLength/60)%60) + ":" + (fullLength%60).ToString("D2");
    }

    void AudioClipOver()
    {
        PlayerPrefs.SetString(MainController.Instance.getCurrentTempleData().name + "/" + symbolName, "discovered");
        playTime = 0;
        audioSouce.Stop();
        stopButton.SetActive(false);
        playButton.SetActive(true);
        ShowPlaytime();
    } 

    public void Rewind()
    {

        playTime = playTime - 10;
        if(playTime < 0)
        {
            playTime = 0;
        }
        audioSouce.time = playTime;
        ShowPlaytime();
    }

    public void Forwind()
    {
        playTime = playTime + 10;
        if (playTime > fullLength)
        {
            playTime = fullLength;
        }
        audioSouce.time = playTime;
        ShowPlaytime();
    }

  
    public void LoadImageDetectionscene()
    {
        SceneManager.LoadScene("ImageDetectionScene");
    }
    
    public void LoadHomePage()
    {
        SceneManager.LoadScene("TempleSelectionScene");
    }


}
