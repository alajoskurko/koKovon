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
    GameObject symbolContainer,symbolGropuContainer;
    [SerializeField]
    GameObject symbolPrefab;
    [SerializeField]
    GameObject symbolGroupPrefab;
    TempleData currentTempleData;
    SymbolGroup[] symbolsGroups;
    Symbol[] symbols;



    private void Awake()
    {
        SwipeDetector.OnSwipe += SwipeDetector_OnSwipe;
    }
    void Start()
    {
       
        MainController.Instance.ResetSymbolTexturesList();
        currentTempleData = MainController.Instance.getCurrentTempleData();
        templeNameText.text = currentTempleData.name;
        symbols = currentTempleData.symbol_groups.csillag.symbols;
        InstantiatewSymbols(symbolContainer);
        //InstantiateSymbolGroups(symbolGropuContainer);
    }

    public void InstantiateSymbolGroups(GameObject parent)
    {
        foreach (SymbolGroup symbol in symbolsGroups)
        {
            var newSymbolItem = Instantiate(symbolGroupPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            if (parent != null)
            {
                newSymbolItem.transform.parent = parent.transform;
                newSymbolItem.transform.localScale = new Vector3(1, 1, 1);

            }
            //newSymbolItem.GetComponent<SymbolGroupPrefabController>().SetSymbolData(symbol);
        }
    }

    public void InstantiatewSymbols(GameObject parent)
    {
        foreach (Symbol symbol in symbols)
        {
            var newSymbolItem = Instantiate(symbolPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            if (parent != null)
            {
                newSymbolItem.transform.parent = parent.transform;
                newSymbolItem.transform.localScale = new Vector3(1, 1, 1);

            }
            newSymbolItem.GetComponent<SymbolPrefabController>().SetSymbolData(symbol);

        }
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
