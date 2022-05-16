using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TempleData;

public class SymbolPrefabController : MonoBehaviour
{

    [SerializeField]
    RawImage symbolImage;
    Symbol symbol;
    [SerializeField]
    Image checkmarkImage;
    // Start is called before the first frame update
    public void SetSymbolData(Symbol symbolData)
    {
        symbol = symbolData;
        ProcessSymbolImage();
    }

    public void ProcessSymbolImage()
    {
        Texture2D imageTexture = new Texture2D(0, 0, TextureFormat.PVRTC_RGBA4, false);
        byte[] resultBytes = MainController.Instance.GetImageLocaly(MainController.Instance.getCurrentTempleData().name, symbol.symbol_name, ".svg");
        imageTexture.LoadImage(resultBytes);
        symbolImage.texture = imageTexture;
        int height = imageTexture.height;
        int width = imageTexture.width;
        float ratio = ((float ) width )/ ((float)height);
       if(ratio <= 1)
        {
            symbolImage.rectTransform.sizeDelta = new Vector2(170 * ratio, 170);
        }
        else
        {
            symbolImage.rectTransform.sizeDelta = new Vector2(170, 170 / ratio);
        }
        
        Color currColor = symbolImage.color;
        currColor.a = 1;
        symbolImage.color = currColor;
      
    }
    //rename and redo it to set color
    public void SetCheckmark()
    {
        checkmarkImage.gameObject.SetActive(PlayerPrefs.HasKey(MainController.Instance.getCurrentTempleData().name + "/" + symbol.symbol_name));
    }
}
