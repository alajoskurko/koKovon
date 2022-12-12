using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static TempleData;

public class SymbolGroupPrefabController : MonoBehaviour
{
    [SerializeField]
    RawImage symbolImage;
    [SerializeField]
    TMPro.TMP_Text _symbolName;
    KeyValuePair<string, SymbolGroup> symbol = new KeyValuePair<string, SymbolGroup>();
    Dictionary<string, string> symbolNameByLang = new Dictionary<string, string>();
    Dictionary<string, Dictionary<string, string>> symbolName = new Dictionary<string, Dictionary<string, string>>()
        {
            { "kor",new Dictionary<string,string>(){ { "hu","Ég" }, { "en", "circle" }, { "ro", "cerc" } } },
            { "negyzet",new Dictionary<string,string>(){ { "hu", "Föld" }, { "en", "circle" }, { "ro", "cerc" } } },
            { "csillag",new Dictionary<string,string>(){ { "hu", "Legenda" }, { "en", "circle" }, { "ro", "cerc" } } },
            { "boltiv",new Dictionary<string,string>(){ { "hu", "Fal" }, { "en", "circle" }, { "ro", "cerc" } } },
        };





    // Start is called before the first frame update
    public void SetSymbolData(KeyValuePair<string, SymbolGroup> symbolData)
    {
        symbol = symbolData;
        ProcessSymbolImage();
    }

    public void ProcessSymbolImage()
    {
        Texture2D imageTexture = new Texture2D(512, 512, TextureFormat.PVRTC_RGBA4, false);
        byte[] resultBytes = MainController.Instance.GetImageLocaly(MainController.Instance.getCurrentTempleData().name, symbol.Key, ".svg");
        //Debug.Log(symbol.Key);
        _symbolName.text = symbolName[symbol.Key][MainController.Instance.selectedLanguage];
        //Debug.Log(symbolName[symbol.Key][MainController.Instance.selectedLanguage]);
        imageTexture.LoadImage(resultBytes);
        symbolImage.texture = imageTexture;
        Color currColor = symbolImage.color;
        currColor.a = 1;
        symbolImage.color = currColor;

    }

    public void ChooseThisSymbolGroup()
    {
        Scene scene = SceneManager.GetActiveScene();
        if(scene.name == "DiscoverScene")
        {
            DiscverSceneController.Instance.ChooseSymbolGroup(symbol.Key);
        }
        else
        {
            TempleSceneController.Instance.ChooseSymbolGroupForScan(symbol);
        }
        
    }
}

