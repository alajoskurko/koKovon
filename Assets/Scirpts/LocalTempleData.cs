using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LocalTempleData 
{
    public string updated_at;
    public string name;
    public bool downloaded;
   
    
    public LocalTempleData(string updated_at,string name, bool downloaded)
    {
        this.updated_at = updated_at;
        this.name = name;
        this.downloaded = downloaded;
    }
}
