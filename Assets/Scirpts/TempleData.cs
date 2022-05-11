using System;
using System.Collections.Generic;

[Serializable]
public class TempleData
{

    public string id;
    public string name;
    public string description;
    public string cover_image;
    public string plan_image;
    public string audio_intro;
    public string created_at;
    public string updated_at;
    public string google_map;
    public ForeignTempleDetails[] temple_details;
    public Dictionary<string,SymbolGroup> symbol_groups = new Dictionary<string,SymbolGroup>();


    [Serializable]
    public class AudioData
    {
        public string path;
        public string lang;
        public string name;
    }

    //public class SymbolGroups
    //{
    //    public SymbolGroup csillag;
    //    public SymbolGroup kor;
    //    public SymbolGroup negyzet;
    //    //public string symbol_path;
    //    //public AudioData[] audios;
    //    //public string symbol_name;

    //}

    [Serializable]
    public class SymbolGroup
    {
        public string path;
        public Symbol[] symbols;
        //public string symbol_path;
        //public AudioData[] audios;
        //public string symbol_name;


    }
    [Serializable]
    public class Symbol
    {
        public string symbol_path;
        public AudioData[] audios;
        public string symbol_name;

    }

    [Serializable]
    public class ForeignTempleDetails
    {
        public string lang;
        public string name;
        public string audio_intro;
        public string description;

    }

}
