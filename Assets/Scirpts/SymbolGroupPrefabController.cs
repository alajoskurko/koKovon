using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TempleData;

public class SymbolGroupPrefabController : MonoBehaviour
{
    [SerializeField]
    RawImage symbolImage;
    SymbolGroup symbol;
    // Start is called before the first frame update
    public void SetSymbolData(SymbolGroup symbolData)
    {
        symbol = symbolData;
        ProcessSymbolImage();
    }

    public void ProcessSymbolImage()
    {
        //Texture2D imageTexture = new Texture2D(512, 512, TextureFormat.PVRTC_RGBA4, false);
        //byte[] resultBytes = MainController.Instance.GetImageLocaly(MainController.Instance.getCurrentTempleData().name, symbol.symbol_name, ".svg");
        //imageTexture.LoadImage(resultBytes);
        //symbolImage.texture = imageTexture;
        //Color currColor = symbolImage.color;
        //currColor.a = 1;
        //symbolImage.color = currColor;

    }
}
