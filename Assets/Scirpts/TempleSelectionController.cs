using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TempleSelectionController : MonoBehaviour
{

    TempleData[] allTempleData;
    Dictionary<string, LocalTempleData> allLocalTempleData;
    [SerializeField]
    GameObject templePrefab;
    [SerializeField]
    GameObject mainTempleContainer;
    [SerializeField]
    GameObject languageChooser;
    [SerializeField]
    Text currentLanguage;

    private void Awake()
    {
        InitTemples();
    }

    void Start()
    {

        InstantiateTempleItems(templePrefab,mainTempleContainer);
        currentLanguage.text = MainController.Instance.selectedLanguage;

    }

    void InitTemples()
    {
        allTempleData = MainController.Instance.LoadAllTempleData();
        allLocalTempleData = MainController.Instance.LoadAllLocalTempledata();
        MainController.Instance.SetAllTempleData(allTempleData);
    }
    
    void InstantiateTempleItems( GameObject prefab, GameObject parent = default(GameObject))
    {
        if(parent.transform.childCount > 0)
        {
            foreach (Transform child in parent.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        Dictionary<string,LocalTempleData> allLocalTempleData = MainController.Instance.LoadAllLocalTempledata();

        foreach(TempleData templeData in allTempleData)
        {
            var newTempleItem = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
            if(parent != null)
            {
                newTempleItem.transform.parent = parent.transform;
                newTempleItem.transform.localScale = new Vector3(1,1,1);
            }
            TempleObjectController templeObjectController = newTempleItem.GetComponent<TempleObjectController>();
            templeObjectController.SetTempleData(templeData, allLocalTempleData[templeData.name].downloaded);
        
        }

        
    }

    public void ShowLanguageChooser()
    {
        if (languageChooser.gameObject.activeInHierarchy)
        {
            languageChooser.gameObject.SetActive(false);
        }
        else
        {
            languageChooser.gameObject.SetActive(true);
        }
        
    }

    public void ChangeLanguage(string language)
    {
        MainController.Instance.ChangeLanguage(language);
        languageChooser.gameObject.SetActive(false);
        currentLanguage.text = language;
        InitTemples();
        InstantiateTempleItems(templePrefab, mainTempleContainer);
    }
  


}
