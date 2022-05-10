using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.IO;
using System;

public class DataController : MonoBehaviour
{

    public void SaveIntoJson(object data, string fileName)
    {
        Debug.Log("Saving to: " + Application.persistentDataPath + "/" + fileName);
        string jsonData = JsonConvert.SerializeObject(data);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/"+fileName, jsonData);
    }

    public string LoadJsonFile(string fileName)
    {
        Debug.Log("Reading from: " + Application.persistentDataPath + "/" + fileName);

        string jsonString= System.IO.File.ReadAllText(Application.persistentDataPath + "/" + fileName);

        return jsonString;
    }

    public void SaveImageLocally(byte[] resultBytes, string templeName, string fileName)
    {
        if(!Directory.Exists(Application.persistentDataPath + "/" + templeName))
        {
            var folder = Directory.CreateDirectory(Application.persistentDataPath + "/" + templeName);
        }
        Debug.Log("Saving to: " + Application.persistentDataPath + "/" + templeName +"/"+fileName+ ".jpg");
        File.WriteAllBytes(Application.persistentDataPath + "/" + templeName +"/"+fileName+ ".jpg", resultBytes);
    }

    public byte[] LoadImageLocally(string templeName,string fileName , string extension = ".jpg")
    {
        return File.ReadAllBytes(Application.persistentDataPath + "/" + templeName + "/" + fileName + ".jpg");
    }

    public void SaveAudioLocally(byte[] resultBytes, string templeName, string fileName)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/" + templeName))
        {
            var folder = Directory.CreateDirectory(Application.persistentDataPath + "/" + templeName);
        }
        Debug.Log("Saving to: " + Application.persistentDataPath + "/" + templeName + "/" + fileName + ".mp3");
        File.WriteAllBytes(Application.persistentDataPath + "/" + templeName + "/" + fileName + ".mp3", resultBytes);
    }
    
    private float[] ConvertByteToFloat(byte[] array)
    {
        float[] floatArr = new float[array.Length / 4];
        for (int i = 0; i < floatArr.Length; i++)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(array, i * 4, 4);
            floatArr[i] = BitConverter.ToSingle(array, i * 4) / 0x80000000;
        }
        return floatArr;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
