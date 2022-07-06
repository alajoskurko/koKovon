using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LocalTempleData 
{
    public string updated_at;
    public string name;
    public Dictionary<string,bool> downloaded;
   
    
    public LocalTempleData(string updated_at,string name, Dictionary<string,bool> downloaded)
    {
        this.updated_at = updated_at;
        this.name = name;
        this.downloaded = downloaded;
    }
}
