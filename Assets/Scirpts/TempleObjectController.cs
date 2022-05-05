using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TempleObjectController : MonoBehaviour
{
    TempleData templeData;
    [SerializeField]
    RawImage templeImage;
    [SerializeField]
    TMP_Text templeNameText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TempleSelected()
    {
        Debug.Log("Selected " + templeData.id);
        MainController.Instance.setCurrentTempleData(templeData);
        SceneManager.LoadScene("SpecificTempleScene");


    }
   

    public void SetTempleData(TempleData newTempleData, bool isDownloaded)
    {
        templeData = newTempleData;

        //Set temple name
        templeNameText.text = templeData.name;

        if (isDownloaded)
        {
            Texture2D imageTexture = new Texture2D(150, 150, TextureFormat.PVRTC_RGBA4, false);

            byte[] resultBytes = MainController.Instance.GetImageLocaly(templeData.name, templeData.name);

            imageTexture.LoadImage(resultBytes);

            templeImage.texture = imageTexture;

        }
        else
        {
            Texture2D imageTexture = new Texture2D(150, 150, TextureFormat.PVRTC_RGBA4, false);

            byte[] resultBytes = MainController.Instance.GetImageLocaly(templeData.name, templeData.name);

            imageTexture.LoadImage(resultBytes);

            templeImage.texture = imageTexture;

            templeImage.color = new Color32(111, 189, 195, 255);
        }
      

        
    }


    public TempleData GetTempleData()
    {
        return templeData;
    }



}
