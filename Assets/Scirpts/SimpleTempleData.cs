using System;
using System.Collections.Generic;

[Serializable]
public class SimpleTempleData
{

    public string id;
    public string name;
   // public string description;
    public string cover_image;
    public string plan_image;
 //   public AudioData[] audios;
    public ForeignTempleData[] languages;
//    public string created_at;


    [Serializable]
    public class ForeignTempleData
    {
        public string language;
        public string name;
        public string description;
        public AudioData[] audios;
    }
    [Serializable]
    public class AudioData
    {
        public string audio_path;
    }

}
