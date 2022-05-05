using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TempleSelectionController : MonoBehaviour
{

    TempleData[] allTempleData;
    Dictionary<string, LocalTempleData> allLocalTempleData;
    [SerializeField]
    GameObject templePrefab;
    [SerializeField]
    GameObject mainTempleContainer;

    private void Awake()
    {
        allTempleData= MainController.Instance.LoadAllTempleData();
        allLocalTempleData = MainController.Instance.LoadAllLocalTempledata();
        MainController.Instance.SetAllTempleData(allTempleData); 

    }

    void Start()
    {

        InstantiateTempleItems(templePrefab,mainTempleContainer);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void InstantiateTempleItems( GameObject prefab, GameObject parent = default(GameObject))
    {
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

  


}
