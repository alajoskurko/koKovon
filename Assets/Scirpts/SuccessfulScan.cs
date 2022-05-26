using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TempleData;

public class SuccessfulScan : MonoBehaviour
{
    [SerializeField]
    GameObject BGPanel, successfullScanPanel,symbolPrefab;

    string scannedSymbolName;
    [SerializeField]
    Image symbolImage;

    TempleData currentTempleData;
    SymbolGroup chosenSymbolGroup;
    Dictionary<string, SymbolGroup> symbolsGroups = new Dictionary<string, SymbolGroup>();
    // Start is called before the first frame update
    void Start()
    {
        currentTempleData = MainController.Instance.getCurrentTempleData();
        symbolsGroups = currentTempleData.symbol_groups;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SuccessfulScanHappened(string scannedSymbol)
    {
        successfullScanPanel.gameObject.SetActive(true);
        BGPanel.gameObject.SetActive(false);
        scannedSymbolName = scannedSymbol;
        CreatSymbol();
    }

    private void CreatSymbol()
    {
        Texture2D imageTexture = new Texture2D(512, 512, TextureFormat.PVRTC_RGBA4, false);
        byte[] resultBytes = MainController.Instance.GetImageLocaly(MainController.Instance.getCurrentTempleData().name, scannedSymbolName, ".jpg");
        imageTexture.LoadImage(resultBytes);
        symbolImage.sprite = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2());

        //newSymbolItem.GetComponent<SymbolPrefabController>().SetSymbolData(currentTempleData.symbol_groups["csillag"].symbols[scannedSymbolName]);
    }
}
