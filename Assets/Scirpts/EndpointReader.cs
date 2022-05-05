using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class EndpointReader : MonoBehaviour
{
  
    public IEnumerator GetTempleData(string id, System.Action<TempleData> callback)
    {
        UnityWebRequest www = UnityWebRequest.Get("https://kokovon.camelcoding.com/temple/"+id);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("There was error loading temple with id: " + id);

        }
        else { 

            TempleData result = JsonConvert.DeserializeObject<TempleData>(www.downloadHandler.text);

            Debug.Log("Sucessfuly read data for temple with id: "+ id);

            callback(result);
        }
    }
    public IEnumerator GetAllTempleData(System.Action<TempleData[]> callback)
    {
        UnityWebRequest www = UnityWebRequest.Get("https://kokovon.camelcoding.com/temples");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("There was an error reading data for temples");

        }
        else
        {

            TempleData[] result = JsonConvert.DeserializeObject<TempleData[]>(www.downloadHandler.text);
          //  MainController.Instance.SetAllTempleData(result);
    //        MainController.Instance.allTempleDataRead = true;
            Debug.Log("Sucessfuly read data for " + result.Length + " temples");
            callback(result);
          
        }
    }

    public IEnumerator GetImage(string path, string name, System.Action<byte[],string> callback)
    {
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("There was an error reading the image");

        }
        else
        {

            byte[] resultBytes = www.downloadHandler.data;
            
         //   var pngFormatedImage = imageTexture.EncodeToPNG();
           
            print("image loaded with success");

            callback(resultBytes,name);

        }
    }

    public IEnumerator GetAudio(string path, string name, System.Action<byte[], string> callback)
    {
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("There was an error reading the image");

        }
        else
        {

            byte[] resultBytes = www.downloadHandler.data;


            print("audio loaded with success");

            callback(resultBytes, name);

        }
    }

   


}
