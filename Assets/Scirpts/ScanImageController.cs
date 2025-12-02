using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TempleData;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScanImageController : MonoBehaviour
{
    [SerializeField]
    GameObject symbolGroupPanel;
    [SerializeField]
    GameObject symbolPrefab;
    TempleData currentTempleData;
    ProgressObject progressObject;
    ProgressController progressController;
    [SerializeField]
    Material scannedSymbolMaterial;

    Symbol[] symbols;
    public static DiscverSceneController Instance;
    KeyValuePair<string, SymbolGroup> chosenSymbolGroup;
    // Start is called before the first frame update
    void Start()
    {
        currentTempleData = MainController.Instance.getCurrentTempleData();
        chosenSymbolGroup = MainController.Instance.chosenSymbol;
        progressController = MainController.Instance.progressController;
        progressObject = progressController.LoadProgressObject();
        
    }
    public void InstantiateAll()
    {
        InstantiateSymbols(symbolGroupPanel.transform.GetChild(0).gameObject, chosenSymbolGroup.Value.symbols);
    }
    public void InstantiateSymbols(GameObject parent, Symbol[] symbolGroup)
    {
        var xPos = -125;
        var yPos = 65;
        var counter = 0;
        foreach (Symbol symbol in symbolGroup)
        {
            counter++;
            var newSymbolItem = Instantiate(symbolPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            if (parent != null)
            {
                newSymbolItem.transform.parent = parent.transform;
                newSymbolItem.transform.localScale = new Vector3(.65f, .65f, .65f);
                newSymbolItem.transform.localPosition = new Vector3(parent.transform.localPosition.x + xPos, yPos, parent.transform.localRotation.z);
            }
            newSymbolItem.GetComponent<SymbolPrefabController>().SetSymbolData(symbol);
            if (ChecIfSymbolIsScanned(symbol.symbol_name))
            {
                //newSymbolItem.transform.GetChild(0).GetComponent<RawImage>().color = new Color32(8, 149, 160, 250);
                newSymbolItem.transform.GetChild(0).GetComponent<RawImage>().material = scannedSymbolMaterial;
            }
            xPos = xPos + 125;
            if (counter == 3)
            {
                xPos = -125;
                yPos = -65;
            }   
        }
    }
    bool ChecIfSymbolIsScanned(string symbolName)
    {
        if (progressObject.scannedSymbols.Count > 0)
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
    // Update is called once per frame
    void Update()
    {
        
    }
}
