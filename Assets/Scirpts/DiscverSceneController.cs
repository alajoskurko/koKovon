using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SwipeDetector;
using static TempleData;

public class DiscverSceneController : MonoBehaviour
{
    [SerializeField]
    Text templeNameText;
    [SerializeField]
    RawImage symbolGroupImage;
    [SerializeField]
    GameObject symbolContainer,symbolGropuContainer;
    [SerializeField]
    GameObject symbolPanel, symbolGroupPanel;
    [SerializeField]
    GameObject symbolPrefab;
    [SerializeField]
    GameObject symbolGroupPrefab;
    [SerializeField]
    GameObject scrollRectForSymbols;
    TempleData currentTempleData;
    Dictionary<string, SymbolGroup> symbolsGroups = new Dictionary<string, SymbolGroup>();

    ProgressObject progressObject;

    ProgressController progressController;
   
    Symbol[] symbols;
    public static DiscverSceneController Instance;
    SymbolGroup chosenSymbolGroup;



    private void Awake()
    {
        SwipeDetector.OnSwipe += SwipeDetector_OnSwipe;
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }
    void Start()
    {
        currentTempleData = MainController.Instance.getCurrentTempleData();
        progressController = MainController.Instance.progressController;
        templeNameText.text = currentTempleData.name;
        symbolsGroups = currentTempleData.symbol_groups;
        progressObject = progressController.LoadProgressObject();
        Debug.Log(progressObject + " progressobject");
        InstantiateSymbolGroups(symbolGropuContainer);

    }
    void InitSymbols()
    {
        MainController.Instance.ResetSymbolTexturesList();
        symbols = chosenSymbolGroup.symbols;
        //InstantiateSymbols(symbolContainer);
        symbolGroupPanel.gameObject.SetActive(false);
    }
    public void InstantiateSymbols(GameObject parent, Symbol[] symbolGroup)
    {
        foreach (Symbol symbol in symbolGroup)
        {
            var newSymbolItem = Instantiate(symbolPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            if (parent != null)
            {
                newSymbolItem.transform.parent = parent.transform;
                newSymbolItem.transform.localScale = new Vector3(1, 1, 1);

            }
            newSymbolItem.GetComponent<SymbolPrefabController>().SetSymbolData(symbol);
            if (ChecIfSymbolIsScanned(symbol.symbol_name))
            {
                newSymbolItem.transform.GetChild(0).GetComponent<RawImage>().color = new Color32(8, 149, 160, 250);
            }

        }
    }

    bool ChecIfSymbolIsScanned(string symbolName)
    {
        if(progressObject.scannedSymbols.Count > 0)
        {
            if (!progressObject.scannedSymbols.ContainsKey(currentTempleData.name))
            {
                return false;
            }
            if (progressObject.scannedSymbols[currentTempleData.name].Contains(symbolName))
            {
                Debug.Log("megvan a " + symbolName);
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public void InstantiateSymbolGroups(GameObject parent)
    {
        int yPos = 200;
        foreach (KeyValuePair<string, SymbolGroup> symbolGroup in symbolsGroups )
        {
            
            var newSymbolScrollRect = Instantiate(scrollRectForSymbols, new Vector3(symbolGroupPanel.transform.position.x, symbolGroupPanel.transform.position.y - yPos, 0), Quaternion.identity);
            if(symbolGroupPanel != null)
            {
                newSymbolScrollRect.transform.parent = symbolGroupPanel.transform;
                newSymbolScrollRect.transform.localScale = new Vector3(1, 1, 1);
            }
            Texture2D imageTexture = new Texture2D(0, 0, TextureFormat.PVRTC_RGBA4, false);
            byte[] resultBytes = MainController.Instance.GetImageLocaly(MainController.Instance.getCurrentTempleData().name, symbolGroup.Key, ".svg");
            imageTexture.LoadImage(resultBytes);
            symbolGroupImage = newSymbolScrollRect.transform.GetChild(0).GetComponentInChildren<RawImage>();
            symbolGroupImage.texture = imageTexture;
            Debug.Log("keeep  " + resultBytes);
            InstantiateSymbols(newSymbolScrollRect.transform.GetChild(1).gameObject, symbolGroup.Value.symbols);
            yPos += 350;
            //var newSymbolItem = Instantiate(symbolGroupPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            //if (parent != null)
            //{
            //    newSymbolItem.transform.parent = parent.transform;
            //    newSymbolItem.transform.localScale = new Vector3(1, 1, 1);

            //}
            //newSymbolItem.GetComponent<SymbolGroupPrefabController>().SetSymbolData(symbolGroup);
        }
    }

    public void ChooseSymbolGroup(string symbolGroupName)
    {
        chosenSymbolGroup = symbolsGroups[symbolGroupName];
        InitSymbols();
    }

    private void SwipeDetector_OnSwipe(SwipeData data)
    {
        if (data.Direction == SwipeDirection.Right)
        {
            LoadSpecificTempleScene();
        }
    }

    public void LoadSpecificTempleScene()
    {
        SwipeDetector.OnSwipe -= SwipeDetector_OnSwipe;
        SceneManager.LoadScene("SpecificTempleScene");
    }




}
