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
    KeyValuePair<string, SymbolGroup> symbol = new KeyValuePair<string, SymbolGroup>();

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
