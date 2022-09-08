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
    [SerializeField]
    ScrollRect scrollrect;
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
        var akarmi = GameObject.Find("TempleContainer").GetComponent<RectTransform>();
        var scrrollrect = GameObject.Find("ScrollRect").GetComponent<ScrollRect>();
        MainController.Instance.templeSelectorScrollrectPositionY = scrrollrect.verticalNormalizedPosition;;
        MainController.Instance.setCurrentTempleData(templeData);
        SceneManager.LoadScene("SpecificTempleScene");
       
    }
   

    public void SetTempleData(TempleData newTempleData, bool isDownloaded)
    {
        templeData = newTempleData;

        if (MainController.Instance.selectedLanguage == "hu")
        {
            //Set temple name
            templeNameText.text = templeData.name;
        }
        else
        {
            foreach (var templeDetail in templeData.temple_details)
            {
                if(templeDetail.lang == MainController.Instance.selectedLanguage)
                {
                    templeNameText.text = templeDetail.name;
                }
            }
        }
        

        if (isDownloaded)
        {
            Texture2D imageTexture = new Texture2D(150, 150, TextureFormat.RGB565, false);


            byte[] resultBytes = MainController.Instance.GetImageLocaly(templeData.name, templeData.name);

            imageTexture.LoadImage(resultBytes);

            templeImage.texture = imageTexture;
            //templeImage.color = new Color32(111, 189, 195, 255);
            templeImage.SetNativeSize();
            if(templeData.name == "Jézus kápolna")
            {
                templeImage.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                templeImage.uvRect = new Rect(0, 0, 1, 0.990f);
            }
            else
            {
                templeImage.transform.localScale = new Vector3(.75f, .75f, .5f);
            }
        }
        else
        {
            Texture2D imageTexture = new Texture2D(150, 150, TextureFormat.RGB565, false);

            byte[] resultBytes = MainController.Instance.GetImageLocaly(templeData.name, templeData.name);

            imageTexture.LoadImage(resultBytes);

            templeImage.texture = imageTexture;

            //templeImage.color = new Color32(111, 189, 195, 255);
            templeImage.SetNativeSize();
            if (templeData.name == "Jézus-kápolna")
            {
                templeImage.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                templeImage.uvRect = new Rect(0, 0, 1, 0.990f);
            }
            else
            {
                templeImage.transform.localScale = new Vector3(.75f, .75f, .5f);
                
            }
        }
      

        
    }


    public TempleData GetTempleData()
    {
        return templeData;
    }



}
