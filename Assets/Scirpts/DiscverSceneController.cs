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
    GameObject symbolPanel, symbolGroupPanel;
    [SerializeField]
    GameObject symbolPrefab;
    [SerializeField]
    GameObject symbolGroupPrefab;
    TempleData currentTempleData;
    Dictionary<string, SymbolGroup> symbolsGroups = new Dictionary<string, SymbolGroup>();
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
        templeNameText.text = currentTempleData.name;
        symbolsGroups = currentTempleData.symbol_groups;
        InstantiateSymbolGroups(symbolGropuContainer);

    }
    void InitSymbols()
    {
        MainController.Instance.ResetSymbolTexturesList();
        symbols = chosenSymbolGroup.symbols;
        InstantiateSymbols(symbolContainer);
        symbolGroupPanel.gameObject.SetActive(false);
    }
    public void InstantiateSymbols(GameObject parent)
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

    public void InstantiateSymbolGroups(GameObject parent)
    {
        foreach (KeyValuePair<string, SymbolGroup> symbol in symbolsGroups )
        {
            var newSymbolItem = Instantiate(symbolGroupPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            if (parent != null)
            {
                newSymbolItem.transform.parent = parent.transform;
                newSymbolItem.transform.localScale = new Vector3(1, 1, 1);

            }
            newSymbolItem.GetComponent<SymbolGroupPrefabController>().SetSymbolData(symbol);
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
