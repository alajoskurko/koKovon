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
    GameObject symbolContainer;
    [SerializeField]
    GameObject symbolPrefab;
    TempleData currentTempleData;
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
        symbols = currentTempleData.symbols;
        InstantiatewSymbols(symbolContainer);
 
  

    }

    // Update is called once per frame
    void Update()
    {
        
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
